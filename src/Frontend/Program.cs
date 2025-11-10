using Common.Client;
using Common.Configuration;
using Common.Local;
using Common.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ConfigureLogging(Environment.GetEnvironmentVariable("ApplicationInsights__ConnectionString"))
    .CreateBootstrapLogger();

const string localEnvironmentName = "Local";
var builderIsLocalEnvironment = builder.Environment.IsEnvironment(localEnvironmentName);

if (builderIsLocalEnvironment)
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;

// Register an HttpClient with a pre-configured base address
builder.Services.AddHttpClient<IApiClient, ApiClient>(client => { client.BaseAddress = new Uri(apiBaseUrl); });

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(options =>
{
    options.SuppressXFrameOptionsHeader = true;
    options.Cookie.Expiration = TimeSpan.Zero;
});

//builder.AddLogging();
    
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

//app.UseLogEnrichment();

// Configure the HTTP request pipeline.
if (!builderIsLocalEnvironment)
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHealthChecks("/health");

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