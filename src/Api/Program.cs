using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Infrastructure.Persistence;
using Common.Local;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CheckerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    //builder.CreateMockAzureAdClient();
}
else
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd")/*,
            configureJwtBearerOptions: options =>
            {
                // Optionally set Audience explicitly if not in config
                var audience = builder.Configuration["AzureAd:Audience"];
                if (!string.IsNullOrEmpty(audience))
                    options.TokenValidationParameters.ValidAudience = audience;
            }*/);
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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

if (app.Environment.IsDevelopment())
{
    /*app.MapPost("/dev/token", (Func<string, string, IEnumerable<Claim>?, string> tokenFactory, TokenRequest req) =>
        {
            var extra = new List<Claim>();
            if (req.Roles is { Count: > 0 }) extra.AddRange(req.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
            if (req.Scopes is { Count: > 0 }) extra.AddRange(req.Scopes.Select(s => new Claim("scp", s)));
            var jwt = tokenFactory(req.Sub ?? "user1", req.Name ?? "Dev User", extra);
            return Results.Ok(new { access_token = jwt, token_type = "Bearer" });
        })
        .WithTags("dev");*/
}
else
{
    app.UseHttpsRedirection();
    
    app.UseAuthentication();
    app.UseAuthorization();
}
    
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CheckerDbContext>();
    dbContext.Database.EnsureCreated(); // Creates DB and tables if they don't exist
}

app.Run();

public record TokenRequest(string? Sub, string? Name, List<string>? Roles, List<string>? Scopes);