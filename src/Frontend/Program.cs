using Common.Client;
using Common.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Get the API URL from appsettings
var apiBaseUrl = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value!;

// Register an HttpClient with a pre-configured base address
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddSingleton<IApiClient, ApiClient>(options =>
{
    var client = options.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient");
    return new ApiClient(client);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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