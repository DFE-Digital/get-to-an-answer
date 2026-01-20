using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Common.Accessibility;
using Common.Client;
using Common.Configuration;
using Common.Extensions;
using Common.Local;
using Common.Logging;
using Common.Telemetry;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.UI;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static System.TimeSpan;
using Serilog;
using HstsOptions = Joonasw.AspNetCore.SecurityHeaders.HstsOptions;

var builder = WebApplication.CreateBuilder(args);

#region Configuration

builder.Services.AddOptions<ScriptOptions>().BindConfiguration(ScriptOptions.Name);

#endregion

const string localEnvironmentName = "Local";
var builderIsLocalEnvironment = builder.Environment.IsEnvironment(localEnvironmentName);

if (builderIsLocalEnvironment)
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
    builder.Services.AddMockAzureAdForMvc();
}

builder.Services.AddCsp(nonceByteAmount: 32);
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    
    // disabled everything else because the Entra ID was failing to store cookies,
    // but when reconfigured the consent cookie was still not being stored
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.Configure<CookieTempDataProviderOptions>(options =>
{
    options.Cookie.IsEssential = true; // Mark the cookie as essential
});

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

if (!builderIsLocalEnvironment)
{
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
}

builder.Services.AddHttpContextAccessor();

#region GTAA Api Client

var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;

builder.Services.AddTransient(sp =>
    new BearerTokenHandler(sp.GetRequiredService<IHttpContextAccessor>(),
        sp.GetRequiredService<ILogger<BearerTokenHandler>>()));

// Register an HttpClient with a pre-configured base address
builder.Services.AddHttpClient<IApiClient, ApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<BearerTokenHandler>();

#endregion

#region Microsoft Graph

builder.Services.AddTransient(sp =>
    new MsGraphHandler(sp.GetRequiredService<IHttpContextAccessor>()));

var msGraphBaseUrl = builder.Configuration.GetSection("MsGraph:BaseUrl").Value!;

builder.Services.AddHttpClient<IMsGraphClient, MsGraphClient>(client => client.BaseAddress = new Uri(msGraphBaseUrl))
    .AddHttpMessageHandler<MsGraphHandler>();

#endregion

#region Blob Storage

var blobStorageConfig = builder.Configuration.GetSection("BlobStorage");
var blobStorageConnectionString = blobStorageConfig.GetValue<string>("ConnectionString")!;
var blobStorageContainerName = blobStorageConfig.GetValue<string>("ContainerName")!;

builder.Services.AddSingleton<IImageStorageClient>(sp => 
    new ImageStorageClient(blobStorageConnectionString, blobStorageContainerName, 
        sp.GetRequiredService<ILogger<ImageStorageClient>>()));

#endregion

#region Sitemap

// Automatically finds the razor pages and generates a sitemap
builder.Services.AddSingleton<SitemapService>();

#endregion

builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI(); 

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Home/Index", "/");
});
    
builder.Services.AddHealthChecks();

var app = builder.Build();
    
#region Content Security (CSP) and Headers

// HSTS
app.UseStrictTransportSecurity(new HstsOptions(FromDays(365), true, true));

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});
    
// Cookie Security
app.UseCookiePolicy();

// Content Security Policy
app.UseCsp(x =>
{
    x.ByDefaultAllow.FromNowhere();

    var config = app.Configuration.GetSection("Csp").Get<CspConfiguration>() ?? new CspConfiguration();
        
    x.AllowScripts
        .FromSelf()
        .AddNonce();

    config.AllowScriptUrls.ForEach(f => x.AllowScripts.From(f));
    config.AllowHashes.ForEach(f => x.AllowScripts.WithHash(f));

    x.AllowStyles
        .FromSelf()
        .AllowUnsafeInline();

    x.AllowManifest
        .FromSelf();

    config.AllowStyleUrls.ForEach(f => x.AllowStyles.From(f));

    x.AllowFonts
        .FromSelf()
        .From("data:");

    config.AllowFontUrls.ForEach(f => x.AllowFonts.From(f));
        
    x.AllowFraming.FromSelf(); // Block framing on other sites, equivalent to X-Frame-Options: DENY
    config.AllowFrameUrls.ForEach(f => x.AllowFraming.From(f));
    config.AllowFrameUrls.ForEach(f => x.AllowFrames.From(f));
    
    // Needs to anywhere because we can't predict the answer destination DfE users will select
    // for questionnaire previews, meaning the expected behaviour will be broken  
    x.AllowFormActions.ToAnywhere();

    x.AllowImages
        .FromSelf()
        .From("data:");
        
    config.AllowImageUrls.ForEach(f => x.AllowImages.From(f));

    x.AllowConnections
        .ToSelf();
        
    config.AllowConnectUrls.ForEach(f => x.AllowConnections.To(f));
        
    if (config.ReportOnly)
    {
        x.SetReportOnly();
    }
});

#endregion

#region Rebrand

SiteConfiguration.Rebrand = app.Configuration.GetValue<bool>("Rebrand") || DateTime.Today >= new DateTime(2025, 6, 25);

#endregion

#region Error Handling

// Configure the HTTP request pipeline.
if (!builderIsLocalEnvironment)
{
    // Configure the HTTP request pipeline.
    app.UseExceptionHandler("/error/404");

    // Handle non-existing routes (404)
    app.UseStatusCodePagesWithReExecute("/error/{0}");
    
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

#endregion

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
else if (builder.Environment.IsDevelopment())
{
    app.UseDevMvcTokenEndpoints();
}

app.MapControllers();

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