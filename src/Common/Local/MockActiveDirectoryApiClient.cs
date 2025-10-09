using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Local;

public static class MockActiveDirectoryApiClient
{
    
}

// Simple options for the mock user
public sealed class MockAzureAdOptions
{
    public string AuthenticationType { get; set; } = "MockAzureAD";
    public string? Name { get; set; } = "Dev User";
    public string? Email { get; set; } = "dev.user@example.test";
    public string? ObjectId { get; set; } = "11111111-2222-3333-4444-555555555555";
    public string? TenantId { get; set; } = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
    public string[] Roles { get; set; } = new[] { "Admin" };
    public string? PreferredUsername { get; set; } = "dev.user@example.test";
    public string? NameIdentifier { get; set; } // if null, will use ObjectId
}

// Authentication handler that always authenticates a mock user
internal sealed class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptions<MockAzureAdOptions> _mockOptions;

    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IOptions<MockAzureAdOptions> mockOptions)
        : base(options, logger, encoder, clock)
    {
        _mockOptions = mockOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var o = _mockOptions.Value;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, o.Name ?? "Dev User"),
            new(ClaimTypes.Email, o.Email ?? "dev.user@example.test"),
            new("tid", "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            new("oid", "11111aaa-22bb-c33c-dd44-eeeee5555ee5"),
            new(ClaimTypes.NameIdentifier, o.NameIdentifier ?? o.ObjectId ?? Guid.NewGuid().ToString()),
            new("http://schemas.microsoft.com/identity/claims/objectidentifier", o.ObjectId ?? Guid.NewGuid().ToString()),
            new("http://schemas.microsoft.com/identity/claims/tenantid", o.TenantId ?? "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            new("preferred_username", o.PreferredUsername ?? o.Email ?? "dev.user@example.test"),
        };

        foreach (var role in o.Roles ?? Array.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(role))
                claims.Add(new(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, o.AuthenticationType);
        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Middleware that injects a fake JWT bearer for API requests if none provided
internal sealed class MockBearerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _headerName = "Authorization";
    private readonly string _mockToken = "Bearer mock-dev-token";

    public MockBearerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // If caller didn't pass any Authorization header, add a fake one to simulate bearer auth
        if (!context.Request.Headers.ContainsKey(_headerName))
        {
            context.Request.Headers[_headerName] = _mockToken;
        }
        await _next(context);
    }
}

public static class MockAzureAdExtensions
{
    private const string MockScheme = "MockScheme";
    private const string MockCookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    // For MVC app: adds a mock cookie auth and a dev-only /dev/login endpoint to sign-in
    public static IServiceCollection AddMockAzureAdForMvc(this IServiceCollection services)
    {
        Action<MockAzureAdOptions> configure = opts =>
        {
            opts.Name = "Dev Admin";
            opts.Email = "dev.admin@example.test";
            opts.Roles = new[] { "Admin" };
        };
        
        services.Configure(configure ?? (_ => { }));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = MockScheme;
                options.DefaultChallengeScheme = MockScheme;
                options.DefaultSignInScheme = MockCookieScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>(MockScheme, _ => { })
            .AddCookie(MockCookieScheme, options =>
            {
                options.LoginPath = "/dev/login";
                options.AccessDeniedPath = "/dev/denied";
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(MockScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }

    // For API app: adds a mock bearer scheme that always authenticates
    public static IServiceCollection AddMockAzureAdForApi(this IServiceCollection services)
    {
        Action<MockAzureAdOptions> configure = opts =>
        {
            opts.Roles = new[] { "Api.Access", "Admin" };
        };
        
        services.Configure(configure ?? (_ => { }));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = MockScheme;
                options.DefaultChallengeScheme = MockScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>(MockScheme, _ => { });

        services.AddAuthorization();

        return services;
    }

    // Map minimal dev endpoints for login/logout and info in MVC
    public static IApplicationBuilder UseMockMvcDevEndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(_ => { });
        
        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/dev/login", async context =>
            {
                // Force authenticate using the mock scheme and issue a cookie
                var result = await context.AuthenticateAsync(MockScheme);
                if (!result.Succeeded || result.Principal == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Mock login failed");
                    return;
                }

                await context.SignInAsync(MockCookieScheme, result.Principal);
                context.Response.Redirect("/");
            });

            endpoints.MapGet("/dev/logout", async context =>
            {
                await context.SignOutAsync(MockCookieScheme);
                context.Response.Redirect("/");
            });

            endpoints.MapGet("/dev/me", async context =>
            {
                var user = context.User;
                var payload = new
                {
                    user.Identity?.IsAuthenticated,
                    user.Identity?.Name,
                    Claims = user.Claims.Select(c => new { c.Type, c.Value })
                };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload, jsonOptions));
            });
        });

        return app;
    }

    // For API: inject a mock bearer if none present
    public static IApplicationBuilder UseMockBearerIfMissing(this IApplicationBuilder app)
    {
        var builder = app.UseMiddleware<MockBearerMiddleware>();

        app.UseAuthentication();
        
        return builder;
    }
}