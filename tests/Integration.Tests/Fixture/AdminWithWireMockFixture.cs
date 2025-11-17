using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Common.Infrastructure.Persistence;
using Common.Local;
using Integration.Tests.Fake;
using Integration.Tests.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.StaticAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Xunit.Abstractions;

namespace Integration.Tests.Fixture;

using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using AdminProgram = Admin.Program;

// Reusable fixture that hosts the Admin app and a WireMock server
public class AdminWithWireMockFixture : CustomWebApplicationFactory<AdminProgram>, IAsyncLifetime
{
    
    public WireMockAadServer? _aad;
    private ITestOutputHelper _output;

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
                ["AzureAd:AllowWebApiToBeAuthorizedByACL"] = "false",
                ["AzureAd:UseTokenLifetime"] = "true",
                ["StaticAssets:Enabled"] = "false",
                ["Logging:LogLevel:Default"] = "Trace",
                ["Logging:LogLevel:Microsoft"] = "Information",
                ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information"
            };
            config.AddInMemoryCollection(dict!);
            
            var assembly = typeof(AdminWithWireMockFixture).Assembly;
            var baseNamespace = assembly.GetName().Name; // e.g., "Integration.Tests"
            IFileProvider embedded = new EmbeddedFileProvider(assembly, baseNamespace);

            config.AddJsonFile(embedded, "appsettings.json", optional: true, reloadOnChange: false);
            config.AddJsonFile(embedded, "appsettings.LocalTest.json", optional: false, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            //Remove & replace Host SeriLog services
            var seriLogService = services.Where(d => d.ServiceType == typeof(ILoggerFactory)).ToList();
            seriLogService.ForEach(seriLogInstance => services.Remove(seriLogInstance));
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            services.AddLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddConsole();
                lb.AddProvider(new TestOutputLoggerProvider(_output));
                lb.SetMinimumLevel(LogLevel.Trace);

                lb.AddDebug().AddProvider(new TestOutputLoggerProvider(_output));
            });
            
            const string secret = "local-test-signing-key-32bytes-minimum!";

            services.Where(d => d.ServiceType == typeof(IConfigureOptions<OpenIdConnectOptions>))
                .ToList().ForEach(p => services.Remove(p));
            
            services.Where(d => d.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>))
                .ToList().ForEach(p => services.Remove(p));
            
            services.Where(d => d.ServiceType == typeof(IPostConfigureOptions<OpenIdConnectOptions>))
                .ToList().ForEach(p => services.Remove(p));
            
            services.Where(d => d.ServiceType == typeof(OpenIdConnectPostConfigureOptions))
                .ToList().ForEach(p => services.Remove(p));
            
            services.Where(d => d.ServiceType == typeof(IConfigureOptions<MicrosoftIdentityOptions>))
                .ToList().ForEach(p => services.Remove(p));
            
            var data = new Dictionary<string, string?>
            {
                ["AzureAd:Instance"] = $"https://{_aad.BaseUrl}/",
                ["AzureAd:TenantId"] = _aad.TenantId,
                ["AzureAd:Authority"] = authority,
                ["AzureAd:Domain"] = "contoso.com",
                ["AzureAd:ClientId"] = "your-client-id",
                ["AzureAd:ValidateAuthority"] = "false", // avoid host checking
                ["AzureAd:RequireHttpsMetadata"] = "false",
                ["AzureAd:AllowWebApiToBeAuthorizedByACL"] = "false",
                ["AzureAd:UseTokenLifetime"] = "true",
                ["StaticAssets:Enabled"] = "false"
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();

            services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, o =>
            {
                // Replace Authority-based validation with deterministic HS256 for tests
                o.ClientId = "your-client-id";
                o.UseTokenLifetime = true;
                o.StateDataFormat = new PlainTextPropertiesFormat();
                o.CallbackPath = "/__test__/signin-cookie";
                o.RequireHttpsMetadata = false;
                o.Authority = $"{_aad.BaseUrl}/{_aad.TenantId}/v2.0";           // ensure no metadata discovery
                o.MetadataAddress = null;
                o.Backchannel = null;
                o.UseSecurityTokenValidator = true;
                o.SaveTokens = true;
                
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };

                o.ProtocolValidator = new MockOpenIdConnectProtocolValidator();
            });
            
            services
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(config.GetSection("AzureAd"));
            
            /*services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });*/
        });

        builder.Configure(app =>
        {
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                
                // Map test endpoints under a dedicated path
                endpoints.MapPost("/__test__/signin-cookie", async context =>
                {
                    // read posted claims + expiry
                    var payload = await context.Request.ReadFromJsonAsync<SignInPayload>() ?? new SignInPayload();

                    var claims = new List<Claim>();
                    if (payload.Claims is not null)
                    {
                        foreach (var kv in payload.Claims)
                            claims.Add(new Claim(kv.Key, kv.Value));
                    }

                    var identity = new ClaimsIdentity(claims, "Test");
                    var principal = new ClaimsPrincipal(identity);

                    AuthenticationProperties props;

                    if (payload.ExpiresInSeconds > 0)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc =
                                DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresInSeconds)
                        };
                    }
                    else
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true
                        };
                    }

                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                });

                endpoints.MapPost("/__test__/expire-cookie", async context =>
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                });
            });
            
            
            
            //app.MapStaticAssets();
        });
        
        builder.UseEnvironment("LocalTest");
    }

    private sealed class SignInPayload
    {
        public Dictionary<string, string>? Claims { get; set; }
        public int ExpiresInSeconds { get; set; } = 120;
    }

    public async Task InitializeAsync()
    {
        // Start WireMock on a dynamic port
        /*WireMock = WireMockServer.Start(new WireMockServerSettings
        {
            StartAdminInterface = true,
            ReadStaticMappings = false,
            UseSSL = false,
            AllowPartialMapping = true
        });*/

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        try
        {
            //if (_aad is { IsStarted: true }) _aad.Stop();
            _aad?.Dispose();
        }
        catch
        {
            /* ignore */
        }

        await Task.CompletedTask;
    }

    public HttpClient CreateClientFollowingRedirects() => CreateClient();

    public HttpClient CreateClientNoRedirects()
    { 
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        return CreateDefaultClient(Server.BaseAddress, new GenericHandler(handler));
    }
    
    public void EnableXunitLogging(ITestOutputHelper output)
    {
        _output = output;
        Server.Services.GetRequiredService<ILoggerFactory>().AddProvider(new TestOutputLoggerProvider(output));
    }
}

public class MockOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
{
    public MockOpenIdConnectProtocolValidator()
    {
        RequireState = false;
        RequireNonce = false;
        RequireStateValidation = false;
    }
    
    public override void ValidateAuthenticationResponse(OpenIdConnectProtocolValidationContext validationContext)
    {
        
    }
}