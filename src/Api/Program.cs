using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Api.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Common.Extensions;
using Common.Infrastructure.Persistence;
using Common.Local;
using Common.Logging;
using Common.Telemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

const string localEnvironmentName = "Local";
var builderIsLocalEnvironment = builder.Environment.IsEnvironment(localEnvironmentName);

if (builderIsLocalEnvironment)
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

Log.Logger = new LoggerConfiguration()
    .ConfigureLogging(Environment.GetEnvironmentVariable("ApplicationInsights__ConnectionString"))
    .CreateBootstrapLogger();

#region Additional Logging and Application Insights

Log.Logger.Information("Starting application");
Log.Logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

builder.Services.AddSerilog((_, lc) => lc.ConfigureLogging(builder.Configuration["ApplicationInsights:ConnectionString"]));

var appInsightsConnectionString = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString");

if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddProcessor<RouteTelemetryProcessor>()
            .AddEntityFrameworkCoreInstrumentation()
        )
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
        )
        .UseAzureMonitor(monitor => monitor.ConnectionString = appInsightsConnectionString);
}

#endregion

builder.Services.AddDbContext<GetToAnAnswerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
                         builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

builder.Services.AddScoped<IQuestionnaireService, QuestionnaireService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IQuestionnaireRunnerService, QuestionnaireRunnerService>();
builder.Services.AddScoped<IQuestionnaireVersionService, QuestionnaireVersionService>();
builder.Services.AddScoped<IContentService, ContentService>();

#region Setup security and headers

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedFor;
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
    options.AllowedHosts = new List<string>
    {
        "*.azurewebsites.net",
        "*.azurefd.net",
        "*.get-to-an-answer.education.gov.uk"
    };
});

#endregion

builder.Services.AddHttpContextAccessor();

#region HTTP Context and Healthchecks

builder.Services.AddHealthChecks();

#endregion

builder.Services.AddControllers()
    .AddDataAnnotationsLocalization();

if (builderIsLocalEnvironment)
{
//    builder.AddLogging();
}

builder.Services.ConfigureHttpJsonOptions(o => { o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

if (builderIsLocalEnvironment)
{
    builder.Services.AddMockAzureAdForApi();
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder);

    // AllowWebApiToBeAuthorizedByACL is an AzureAd setting in the appsettings.json
    // This avoids needing the jwt token from needing a role or scope
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info = new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Get To An Answer API",
            Version = "v1"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (builderIsLocalEnvironment)
{
//    app.UseLogEnrichment();
}

var appIsLocalEnvironment = app.Environment.IsEnvironment(localEnvironmentName);

var apiName = "Get To An Answer API";

// Serve OpenAPI JSON at /openapi/v1.json
app.MapOpenApi();
app.MapScalarApiReference(options => 
{
    options.WithTitle(apiName);
    options.WithTheme(ScalarTheme.BluePlanet);
    options.HideSidebar();
    options.AddPreferredSecuritySchemes("OAuth2")
        .AddOAuth2Flows("OAuth2", flows =>
        {
            flows.AuthorizationCode = new AuthorizationCodeFlow
            {
                ClientId = "web-client-12345",
                AuthorizationUrl = "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/authorize",
                TokenUrl = "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token",
            };
            flows.ClientCredentials = new ClientCredentialsFlow
            {
                ClientId = "service-client-67890",
                ClientSecret = "service-secret",
                TokenUrl = "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token",
            };
        })
        .AddDefaultScopes("OAuth2", ["api://client-id", "client-id"]);
});

app.MapHealthChecks("/health");

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

#region Database migrations

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GetToAnAnswerDbContext>();
        
        // Only run if there are pending migrations
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        // Decide how you want to handle failures:
        // - log and continue (risky if schema mismatch)
        // - log and rethrow (fail fast)
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw; // recommended in most cases
    }
}

#endregion

if (!appIsLocalEnvironment)
{
    //app.UseHttpsRedirection();
    app.UseForwardedHeaders();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();

namespace Api
{

    [ExcludeFromCodeCoverage]
    public partial class Program
    {
        protected Program()
        {
        }
    }
}