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
}