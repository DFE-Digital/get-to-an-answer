using System.Security.Claims;
using System.Text;
using Common.Infrastructure.Persistence;
using Common.Logging;
using Integration.Tests.Fake;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
// Add Testcontainers namespaces
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Testcontainers.MsSql;
using ApiProgram = Program;

namespace Integration.Tests.Fixture;

public class ApiFixture : WebApplicationFactory<ApiProgram>, IAsyncLifetime
{
    private WireMockAadServer? _aad;
    
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _aad = new WireMockAadServer(); // starts server
        var authority = $"{_aad.BaseUrl}/{_aad.TenantId}/v2.0";

        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
                // Adjust keys to match your app’s auth config
                ["AzureAd:Instance"] = $"{_aad.BaseUrl}/",
                ["AzureAd:TenantId"] = _aad.TenantId,
                ["AzureAd:Authority"] = authority,
                ["AzureAd:Domain"] = "contoso.com",
                ["AzureAd:ClientId"] = "your-client-id",
                ["AzureAd:ValidateAuthority"] = "false", // avoid host checking
                ["AzureAd:RequireHttpsMetadata"] = "false",
                ["AzureAd:AllowWebApiToBeAuthorizedByACL"] = "false"
            };
            config.AddInMemoryCollection(dict!);
            
            var env = "LocalTest";
            config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            // Removes the AddDbContext<GetToAnAnswerDbContext> in the Api program.cs
            var providers = services
                .Where(d => 
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<GetToAnAnswerDbContext>))
                .ToList();
            foreach (var p in providers)
            {
                services.Remove(p);
            }
            
            services.AddDbContext<GetToAnAnswerDbContext>(o =>
            {
                o.UseSqlServer(_msSqlContainer.GetConnectionString());
                o.EnableSensitiveDataLogging();
            });
            
            //Remove & replace Host SeriLog services
            var seriLogService = services.Where(d => d.ServiceType == typeof(ILoggerFactory)).ToList();
            seriLogService.ForEach(seriLogInstance => services.Remove(seriLogInstance));

            services.AddSerilog((_, lc) => lc.ConfigureLogging(""), preserveStaticLogger: true);
            
            const string secret = "local-test-signing-key-32bytes-minimum!";

            var actions = services.Where(d => d.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>)).ToList();
            foreach (var p in actions)
            {
                services.Remove(p);
            }
            
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, o =>
            {
                // Replace Authority-based validation with deterministic HS256 for tests
                o.RequireHttpsMetadata = false;
                o.Authority = null;           // ensure no metadata discovery
                o.MetadataAddress = null;
                o.Backchannel = null;

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };

                // Clear events that might have been added by Microsoft Identity
                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var principal = context.Principal!;
                        // Custom audience check
                        var aud = principal.FindFirst("aud")?.Value;
                        if (aud != "test-audience")
                        {
                            context.Fail("invalid audience");
                            return Task.CompletedTask;
                        }
                        
                        var issuer = principal.FindFirst("iss")?.Value;
                        if (issuer != "http://dfe-issuer")
                        {
                            context.Fail("forbidden access");
                            return Task.CompletedTask;
                        }

                        // Email presence
                        var email = principal.FindFirst(ClaimTypes.Email)?.Value
                                    ?? principal.FindFirst("preferred_username")?.Value;
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            context.Fail("email missing");
                        }

                        return Task.CompletedTask;
                    }
                };
            });
            
            services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });
        });
                
        builder.UseEnvironment("LocalTest");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _aad?.Dispose();
    }

    public Task InitializeAsync()
    {
        return _msSqlContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _msSqlContainer.DisposeAsync().AsTask();
    }


    // Start/stop MSSQL container for the test lifecycle
    /*public async Task InitializeAsync()
    {
        _sqlContainer = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Password = "yourStrong(!)Password"
            })
            .WithCleanUp(true)
            .Build();

        await _sqlContainer.StartAsync();

        _connectionString = _sqlContainer.ConnectionString;

        // Ensure schema exists before tests run
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GetToAnAnswerDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_sqlContainer is not null)
        {
            await _sqlContainer.DisposeAsync();
        }
    }*/
}