using Azure.Monitor.OpenTelemetry.AspNetCore;
using Common.Client;
using Common.Configuration;
using Common.Local;
using Common.Logging;
using Common.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

const string localEnvironmentName = "Local";
var builderIsLocalEnvironment = builder.Environment.IsEnvironment(localEnvironmentName);

if (builderIsLocalEnvironment)
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();
}

builder.Services.AddHttpContextAccessor();

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

#region GTAA Api Client

var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;

// Register an HttpClient with a pre-configured base address
builder.Services.AddHttpClient<IApiClient, ApiClient>(client => { client.BaseAddress = new Uri(apiBaseUrl); });

#endregion

#region Blob Storage

var blogStorageConfig = builder.Configuration.GetSection("BlobStorage");
var blobStorageConnectionString = blogStorageConfig.GetValue<string>("ConnectionString")!;
var blobStorageContainerName = blogStorageConfig.GetValue<string>("ContainerName")!;

builder.Services.AddSingleton<IImageStorageClient>(sp => 
    new ImageStorageClient(blobStorageConnectionString, blobStorageContainerName, 
        sp.GetRequiredService<ILogger<ImageStorageClient>>()));

#endregion

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(options =>
{
    options.SuppressXFrameOptionsHeader = true;
    options.Cookie.Expiration = TimeSpan.Zero;
});
    
builder.Services.AddHealthChecks();

var app = builder.Build();

#region Rebrand

SiteConfiguration.Rebrand = app.Configuration.GetValue<bool>("Rebrand") || DateTime.Today >= new DateTime(2025, 6, 25);

#endregion

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("X-Frame-Options");
    
    context.Response.Headers["Content-Security-Policy"] = "frame-ancestors *";
    //context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    //context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

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

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapStaticAssets();
app.MapRazorPages();

app.Run();