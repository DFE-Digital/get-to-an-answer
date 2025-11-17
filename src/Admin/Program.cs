using System.Diagnostics.CodeAnalysis;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Common.Client;
using Common.Configuration;
using Common.Extensions;
using Common.Local;
using Common.Logging;
using Common.Telemetry;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

const string localEnvironmentName = "Local";
var builderIsLocalEnvironment = builder.Environment.IsEnvironment(localEnvironmentName);

if (builderIsLocalEnvironment)
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true);
    builder.Services.AddMockAzureAdForMvc();
}

var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;
var apiScopes = builder.Configuration.GetSection("ApiSettings:Scopes").Get<string[]>() ?? [];

if (!builderIsLocalEnvironment)
{
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownProxies.Clear();
        options.KnownNetworks.Clear();
        options.AllowedHosts = new List<string>
        {
            "*.azurewebsites.net",
            "*.azurefd.net",
            "*.get-to-an-answer.education.gov.uk"
        };
    });
}

Log.Logger = new LoggerConfiguration()
    .ConfigureLogging(Environment.GetEnvironmentVariable("ApplicationInsights__ConnectionString"))
    .CreateBootstrapLogger();
    
#region Additional Logging and Application Insights
    
Log.Logger.Information("Starting application");
Log.Logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
    
builder.Services.AddSerilog((_, lc) => lc
    .ConfigureLogging(builder.Configuration["ApplicationInsights:ConnectionString"]));

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

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient(sp =>
    new BearerTokenHandler(sp.GetRequiredService<IHttpContextAccessor>()));

// Register an HttpClient with a pre-configured base address
builder.Services.AddHttpClient<IApiClient, ApiClient>(client => { client.BaseAddress = new Uri(apiBaseUrl); })
    .AddHttpMessageHandler<BearerTokenHandler>();

// TODO remove and test no regression
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI(); 

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Home/Index", "/");
    options.Conventions.AddPageRoute("/Shared/Error", "/error");
});

//builder.AddLogging();
    
builder.Services.AddHealthChecks();

var app = builder.Build();

#region Rebrand

SiteConfiguration.Rebrand = app.Configuration.GetValue<bool>("Rebrand") || DateTime.Today >= new DateTime(2025, 6, 25);

#endregion

//app.UseLogEnrichment();

// Configure the HTTP request pipeline.
if (!builderIsLocalEnvironment)
{
    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHealthChecks("/health");

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (builderIsLocalEnvironment)
{
    app.UseMockMvcDevEndpoints();
}

app.MapStaticAssets();
app.MapRazorPages();

app.Run();

namespace Admin
{
    [ExcludeFromCodeCoverage]
    public partial class Program
    {
        protected Program()
        {
        }
    }
}