using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Integration.Tests.Fake;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
// ... existing code ...
using ApiProgram = Program;

namespace Integration.Tests.Fixture;

public class ApiFixture : WebApplicationFactory<ApiProgram>
{
    private WireMockAadServer? _aad;

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
                ["AzureAd:RequireHttpsMetadata"] = "false"
            };
            config.AddInMemoryCollection(dict!);
        });

        builder.ConfigureServices(services =>
        {
            // If your app pre-registers JwtBearerOptions, ensure authority is set to our mock
            const string secret = "local-test-signing-key-32bytes-minimum!";

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
}