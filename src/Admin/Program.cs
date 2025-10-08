using Admin.Client;
using Admin.Configuration;
using Common.Infrastructure.Persistence;
using Common.Local;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Get the API URL from appsettings
var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;

// Register an HttpClient with a pre-configured base address
var httpClientBuilder = builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

if (builder.Environment.IsDevelopment())
{
    //httpClientBuilder.AddHttpMessageHandler(() => new DevTokenHandler(new Uri(apiBaseUrl))); // same host as API;
}

builder.Services.AddSingleton<IApiClient, ApiClient>(options =>
{
    var client = options.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient");
    return new ApiClient(client);
});

if (builder.Environment.IsDevelopment())
{
    //builder.CreateMockAzureAdClient();
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
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

#region Rebrand

SiteConfiguration.Rebrand = app.Configuration.GetValue<bool>("Rebrand") || DateTime.Today >= new DateTime(2025, 6, 25);

#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();