using Common.Client;
using Common.Client;
using Common.Local;
using Common.Logging;
using Common.Telemetry;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddDownstreamApi("Api", options =>
        {
            options.BaseUrl = apiBaseUrl;
            options.Scopes = [$"api://{builder.Configuration["AzureAd:ClientId"]}/.default"];
        })
        .AddInMemoryTokenCaches();
}

builder.Services.AddHttpContextAccessor();

if (builderIsLocalEnvironment)
{
    builder.Services.AddSingleton<ITokenAcquisition, MockTokenAcquisition>();
}

builder.Services.AddTransient(sp =>
    new BearerTokenHandler(
        sp.GetRequiredService<ITokenAcquisition>(),
        [$"api://{builder.Configuration["AzureAd:ClientId"]}/.default"]));

// Register an HttpClient with a pre-configured base address
builder.Services.AddHttpClient<IApiClient, ApiClient>(client => { client.BaseAddress = new Uri(apiBaseUrl); })
    .AddHttpMessageHandler<BearerTokenHandler>();

// Add services to the container.
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
builder.Services.AddRazorPages();

//builder.AddLogging();

var app = builder.Build();

//app.UseLogEnrichment();

// Configure the HTTP request pipeline.
if (!builderIsLocalEnvironment)
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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