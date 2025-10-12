using Common.Client;
using Common.Configuration;
using Common.Local;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
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

// Get the API URL from appsettings
var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;
var apiScopes = builder.Configuration.GetSection("ApiSettings:Scopes").Get<string[]>() ?? [];

builder.Services.AddTransient(sp =>
    new BearerTokenHandler(
        sp.GetRequiredService<ITokenAcquisition>(),
        apiScopes));

// Register an HttpClient with a pre-configured base address
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<BearerTokenHandler>();

builder.Services.AddSingleton<IApiClient, ApiClient>(options =>
{
    var client = options.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient");
    return new ApiClient(client);
});

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

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseMockMvcDevEndpoints();
}

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();