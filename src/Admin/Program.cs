using Common.Client;
using Common.Local;
using Common.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

const string localEnvironmentName = "Local";
var builderIsLocalEnvironment = builder.Environment.IsEnvironment(localEnvironmentName);

if (builderIsLocalEnvironment)
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true);
    builder.Services.AddMockAzureAdForMvc();
}
else
{
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;
var apiScopes = builder.Configuration.GetSection("ApiSettings:Scopes").Get<string[]>() ?? [];

builder.Services.AddTransient(sp =>
    new BearerTokenHandler(
        sp.GetRequiredService<ITokenAcquisition>(),
        apiScopes));

builder.Services.AddHttpClient<IApiClient, ApiClient>(client => { client.BaseAddress = new Uri(apiBaseUrl); })
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.AddLogging();

var app = builder.Build();

app.UseLogEnrichment();

// Configure the HTTP request pipeline.
if (builderIsLocalEnvironment)
{
    app.UseMockBearerIfMissing();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapStaticAssets();

//TODO: do we need?
// app.UseStaticFiles(new StaticFileOptions()
// {
//     OnPrepareResponse = ctx =>
//     {
//         ctx.Context.Response.Headers.Append(
//             "Cache-Control", $"public, max-age={FromDays(31).TotalSeconds}");
//     }
// });

app.UseRouting();

// app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();