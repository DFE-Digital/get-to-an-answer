using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Api.Services;
using Common.Infrastructure.Persistence;
using Common.Local;
using Common.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

const string localEnvironmentName = "Local";
var builderIsLocalEnvironment = builder.Environment.IsEnvironment(localEnvironmentName);

if (builderIsLocalEnvironment)
{
    builder.Configuration
        .AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

builder.Services.AddDbContext<GetToAnAnswerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                         builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

builder.Services.AddScoped<IQuestionnaireService, QuestionnaireService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IQuestionnaireRunnerService, QuestionnaireRunnerService>();
builder.Services.AddScoped<IQuestionnaireVersionService, QuestionnaireVersionService>();
builder.Services.AddScoped<IContentService, ContentService>();

#region Setup security and headers

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedFor;
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
    options.AllowedHosts = new List<string>
    {
        "*.azurewebsites.net",
        "*.azurefd.net",
        "*.get-to-an-answer.education.gov.uk"
    };
});
    
#endregion

#region HTTP Context and Healthchecks
    
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
    
#endregion

builder.Services.AddControllers()
    .AddDataAnnotationsLocalization();

if (builderIsLocalEnvironment) 
{
    builder.AddLogging();
}

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (builderIsLocalEnvironment)
{
    builder.Services.AddHealthChecks();
    builder.Services.AddMockAzureAdForApi();
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    
    // AllowWebApiToBeAuthorizedByACL is an AzureAd setting in the appsettings.json
    // This avoids needing the jwt token from needing a role or scope
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info = new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Get To An Answer API",
            Version = "v1"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (builderIsLocalEnvironment)
{
    app.UseLogEnrichment();
}

var appIsLocalEnvironment = app.Environment.IsEnvironment(localEnvironmentName);

// Configure the HTTP request pipeline.
if (appIsLocalEnvironment)
{
    // Serve OpenAPI JSON at /openapi/v1.json
    app.MapOpenApi();
    app.MapScalarApiReference(options => 
    {
        options.WithTitle("My API");
        options.WithTheme(ScalarTheme.BluePlanet);
        options.HideSidebar();
        options.AddPreferredSecuritySchemes("Bearer");
    });

    // Expose health check endpoint only in Local environment
    app.MapHealthChecks("/health");
}
else
{
    app.MapGroup("/openapi")
       .RequireAuthorization()
       .MapOpenApi();
    app.MapScalarApiReference(); // TODO: Add config
}

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GetToAnAnswerDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (!appIsLocalEnvironment)
{
    //app.UseHttpsRedirection();
    app.UseForwardedHeaders();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program() { }
}