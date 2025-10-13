using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Services;
using Common.Infrastructure.Persistence;
using Common.Local;
using Contentful.AspNetCore.MiddleWare;
using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

Log.Logger = new LoggerConfiguration()
    //.ConfigureLogging(Environment.GetEnvironmentVariable("ApplicationInsights__ConnectionString"))
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GetToAnAnswerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers(); // Enables controller support
builder.Services.AddEndpointsApiExplorer(); // For Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "A simple example ASP.NET Core Web API"
    });
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddMockAzureAdForApi();
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseMockBearerIfMissing(); // optional: inject bearer when missing
}
app.UseAuthorization();
    
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GetToAnAnswerDbContext>();
    dbContext.Database.EnsureCreated();

    // Optional: increase timeout for long migrations
    dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
    
    // Check pending migrations (optional logging/short-circuit)
    var pending = await dbContext.Database.GetPendingMigrationsAsync();
    if (pending.Any())
    {
        // log pending
        await dbContext.Database.MigrateAsync();
    }
}

try
{
    
    #region Contentful Setup

    var contentfulSection = builder.Configuration.GetSection("Contentful");
    builder.Services.AddSingleton(sp =>
    {
        var httpClient = new HttpClient();
        var options = new ContentfulOptions
        {
            DeliveryApiKey = contentfulSection["DeliveryApiKey"],
            PreviewApiKey = contentfulSection["PreviewApiKey"],
            SpaceId = contentfulSection["SpaceId"],
            Environment = contentfulSection["EnvironmentId"],
            UsePreviewApi = bool.TryParse(contentfulSection["UsePreview"], out var usePreview) && usePreview
        };
        return new ContentfulClient(httpClient, options);
    });
    // Register service
    builder.Services.AddScoped<IContentfulSyncService, ContentfulSyncServiceImpl>();

    #endregion

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}