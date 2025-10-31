using System.Diagnostics.CodeAnalysis;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Common.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace Common.Logging;

[ExcludeFromCodeCoverage(Justification = "Configuration only")]
public static class LoggingConfigurationExtensions
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        Log.Logger.Information("Starting application");
        Log.Logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
        
        var appInsightsConnectionString = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString");

        var loggerConfig = builder.Services.AddSerilog((_, lc) =>
        {
            var config = lc
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
        });
        
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddProcessor<RouteTelemetryProcessor>()
                )
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                )
                .UseAzureMonitor(monitor => monitor.ConnectionString = appInsightsConnectionString);
        }

        return builder;
    }
}