using System.Diagnostics.CodeAnalysis;
using Common.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<GetToAnAnswerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers()
    .AddDataAnnotationsLocalization();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
    
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GetToAnAnswerDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

//app.UseAuthentication();
app.UseAuthorization();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program() { }
}