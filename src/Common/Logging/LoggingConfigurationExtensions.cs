using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Common.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Common.Logging;

[ExcludeFromCodeCoverage(Justification = "Configuration only")]
public static class LoggingConfigurationExtensions
{
    public static LoggerConfiguration ConfigureLogging(
        this LoggerConfiguration loggerConfig,
        string? appInsightsConnectionString)
    {
        var config = loggerConfig
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console()
            .Enrich.FromLogContext();

        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            config = config.WriteTo.ApplicationInsights(new TelemetryConfiguration
            {
                ConnectionString = appInsightsConnectionString
            }, TelemetryConverter.Traces);
        }

        return config;
    }
    
    // This method wires up "Azure Silver" level monitoring:
    // - Structured logs to console and Application Insights
    // - Distributed tracing (automatic spans for HTTP, DB, etc.)
    // - Useful metrics (HTTP server, HTTP client, runtime/process)
    // - Azure Monitor export (unified export for logs, traces, metrics)
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        // App Insights connection string is the single knob to turn on Azure Monitor export
        var appInsightsConnectionString = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString");

        // Optional: service identity for better cross-resource correlation and filtering in Azure
        var serviceName = builder.Configuration.GetValue<string>("Service:Name") ?? builder.Environment.ApplicationName;
        var serviceNamespace = builder.Configuration.GetValue<string>("Service:Namespace");
        var serviceVersion = builder.Configuration.GetValue<string>("Service:Version") ?? "1.0.0";

        // Resource describes "who" is producing telemetry (shows up as service.name etc. in Azure)
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion, serviceNamespace: serviceNamespace)
            .AddTelemetrySdk()
            .AddAttributes([
                new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
            ]);

        // Serilog for structured logging:
        // - Keep console for local/dev
        // - Send logs to Application Insights when a connection string is present
        // - Initialize Serilog only if not already initialized
        if (Log.Logger is not Logger || Log.Logger.GetType().Name == "SilentLogger")
        {
            var lc = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.Console()
                .Enrich.FromLogContext();

            if (!string.IsNullOrEmpty(appInsightsConnectionString))
            {
                lc = lc.WriteTo.ApplicationInsights(new TelemetryConfiguration
                {
                    ConnectionString = appInsightsConnectionString
                }, TelemetryConverter.Traces);
            }

            Log.Logger = lc.CreateBootstrapLogger();
        }

        // Replace default Microsoft logging with OpenTelemetry logger provider so:
        // - scopes and state are captured as structured properties
        // - logs correlate with traces by sharing trace/span ids
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger, dispose: false);
        
        builder.Logging.AddConsole();
        builder.Logging.AddOpenTelemetry(otlp =>
        {
            otlp.IncludeScopes = true;      // e.g., "RequestId", "UserId" from logging scopes
            otlp.ParseStateValues = true;   // turn state into structured fields
            otlp.SetResourceBuilder(resourceBuilder);
        });

        // Traces and Metrics via OpenTelemetry + Azure Monitor exporter
        // Tracing gives us distributed traces and spans; metrics cover standard runtime/process/HTTP stats.
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            builder.Services.AddOpenTelemetry()
                // Ensure all signals share the same resource (service.name, version, env)
                .ConfigureResource(r => r.AddService(serviceName, serviceVersion: serviceVersion, serviceNamespace: serviceNamespace)
                    .AddAttributes([
                        new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
                    ]))

                // Tracing: automatic instrumentation + lightweight enrichment
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        // Capture exceptions on spans automatically
                        o.RecordException = true;

                        // Add a couple of useful HTTP tags to requests (visible in App Insights traces)
                        o.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
                        };
                        o.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response_content_length", response.ContentLength);
                        };
                    })
                    .AddHttpClientInstrumentation(o => o.RecordException = true) // outbound HTTP spans
                    .AddEntityFrameworkCoreInstrumentation(o =>
                    {
                        // Helps troubleshooting DB issues; statement text is useful for debugging
                        o.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag("db.system", command.Connection?.GetType().Name);
                        };
                    })
                    // Custom processor for route normalization or extra tags on incoming requests
                    .AddProcessor<RouteTelemetryProcessor>()
                )

                // Metrics: capture standard server/client, runtime/process metrics for dashboards and alerts
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation() // request duration, active requests, etc.
                    .AddHttpClientInstrumentation()  // dependency metrics for outbound calls
                    .AddRuntimeInstrumentation()     // GC, allocations, threadpool
                    .AddProcessInstrumentation()     // CPU, memory, handles
                )

                // Azure Monitor exporter:
                // - Sends traces and metrics (and optionally logs) to Application Insights/Log Analytics
                .UseAzureMonitor(options => { options.ConnectionString = appInsightsConnectionString; });
        }
        
        // Basic startup logs (visible in console and, if configured, App Insights)
        Log.Logger.Information("Starting application");
        Log.Logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

        return builder;
    }
    
    public static IApplicationBuilder UseLogEnrichment(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var activity = Activity.Current;
            var user = context.User;

            var oid = user.FindFirstValue("oid");
            var sub = user.FindFirstValue("sub");
            var tid = user.FindFirstValue("tid");
            var appid = user.FindFirstValue("appid"); // for app roles / client creds

            using (context.RequestServices.GetRequiredService<ILoggerFactory>()
                       .CreateLogger("LogEnrichment")
                       .BeginScope(new Dictionary<string, object?>
                       {
                           ["traceId"] = activity?.TraceId.ToString(),
                           ["spanId"] = activity?.SpanId.ToString(),
                           ["aad.oid"] = oid,
                           ["aad.sub"] = sub,
                           ["aad.tid"] = tid,
                           ["aad.appid"] = appid
                       }))
            {
                await next();
            }
        });
    }
}