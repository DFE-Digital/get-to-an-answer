using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.IdentityModel.Tokens;

namespace Common.Local;

public static class MockActiveDirectoryApiClient
{
    public static void Deconstruct<T>(this T[] array, out T? first, out T? second)
    {
        first = array.Length > 0 ? array[0] : default;
        second = array.Length > 1 ? array[1] : default;
    }
}

// Simple options for the mock user
public sealed class MockAzureAdOptions
{
    public string? Name { get; set; } = "Dev User";
    public string? Email { get; set; } = "dev.user@example.test";
    public string[] Roles { get; set; } = new[] { "Admin" };
}

// Authentication handler that always authenticates a mock user
internal sealed class MockAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    IOptionsMonitor<CookieAuthenticationOptions> cookieOptions,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock,
    IOptions<MockAzureAdOptions> mockOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Request.Cookies.TryGetValue(".MockAuth", out var cookie);

        // Gets the cookie from the browser and extracts the ticket
        var t = new MockAzureAdExtensions.PlainTextTicketFormat().Unprotect(cookie);

        if (t?.Principal.Identity?.IsAuthenticated ?? false)
        {
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(t.Principal, new AuthenticationProperties(), Scheme.Name)));
        }
        
        Request.Headers.TryGetValue("Authorization", out var authHeader);
        
        var (scheme, token) = authHeader.ToString().Split(" ");
        
        if (scheme is null || token is null)
        {
            Request.Query.TryGetValue("jt", out var jwtTokenString);
            
            scheme = "Bearer";
            token = jwtTokenString;
        }

        if (scheme != "Bearer" || string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("invalid auth header"));
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToDictionary(claim => claim.Type, claim => claim.Value );

            var expiration = claims[ClaimTypes.Expiration];
            
            // if expiration is in the future, it's not valid'
            Console.WriteLine($"Expiration: {DateTimeOffset.Parse(expiration)}");
            Console.WriteLine($"Now: {DateTimeOffset.UtcNow}");
            
            if (DateTimeOffset.Parse(expiration) < DateTimeOffset.UtcNow)
                return Task.FromResult(AuthenticateResult.Fail("token expired"));
            
            foreach (var (type, value) in claims)
            {
                Console.WriteLine($"{type}: {value}");
            }
            
            var identity = new ClaimsIdentity(jwtToken.Claims, "MockAzureAD");
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AuthenticateResult.Fail(ex));
        }
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
        
        services.Configure(configure);
        
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = MockScheme;
                options.DefaultAuthenticateScheme = MockScheme;
                options.DefaultChallengeScheme = MockScheme;
                options.DefaultSignInScheme = MockCookieScheme;

            })
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>(MockScheme, _ => { })
            .AddCookie(MockCookieScheme, cookie =>
            {
                cookie.TicketDataFormat = new PlainTextTicketFormat();
                cookie.LoginPath = "/dev/login";
                cookie.LogoutPath = "/dev/logout";
                cookie.Cookie.Name = ".MockAuth";
                cookie.SlidingExpiration = true;
                cookie.ExpireTimeSpan = TimeSpan.FromHours(8);
                cookie.DataProtectionProvider = null;
            });
        
        services.AddSingleton<ITokenAcquisition, MockTokenAcquisition>();
        
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddSingleton<IMsalTokenCacheProvider, MsalMemoryTokenCacheProvider>();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(MockScheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
    
    public sealed class PlainTextTicketFormat : ISecureDataFormat<AuthenticationTicket>
    {
        public string Protect(AuthenticationTicket data)
            => Convert.ToBase64String(
                TicketSerializer.Default.Serialize(data));

        public string Protect(AuthenticationTicket data, string? purpose) => Protect(data);

        public AuthenticationTicket? Unprotect(string? protectedText)
        {
            if (protectedText == null)
                return null;

            var plainText = Convert.FromBase64String(protectedText);
            
            return TicketSerializer.Default.Deserialize(plainText);
        }

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose) => Unprotect(protectedText);
    }

    // For API app: adds a mock bearer scheme that always authenticates
    public static IServiceCollection AddMockAzureAdForApi(this IServiceCollection services)
    {
        Action<MockAzureAdOptions> configure = opts =>
        {
            opts.Roles = new[] { "Api.Access", "Admin" };
        };
        
        services.Configure(configure);

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/dev/login", async context =>
            {
                // Authenticate using the mock scheme to build a principal
                var result = await context.AuthenticateAsync(MockScheme);
                if (!result.Succeeded || result.Principal == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Mock login failed");
                    return;
                }

                // Issue cookie
                await context.SignInAsync(MockCookieScheme, result.Principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                });
                context.Response.Redirect("/");
            }).AllowAnonymous();

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

    private static ClaimsPrincipal? ToClaims(string jwtTokenString)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(jwtTokenString);

            var identity = new ClaimsIdentity(jwtToken.Claims, "MockAzureAD");
            return new ClaimsPrincipal(identity);
            
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    // For API: inject a mock bearer if none present
    public static IApplicationBuilder UseMockBearerIfMissing(this IApplicationBuilder app)
    {
        var builder = app.UseMiddleware<MockBearerMiddleware>();

        app.UseAuthentication();
        
        return builder;
    }
}

public sealed class MockTokenAcquisition : ITokenAcquisition
{
    private static readonly AuthenticationResult MockAuthenticationResult = new(
        accessToken: "mock-user-access-token",
        isExtendedLifeTimeToken: false,
        uniqueId: "mock-unique-id",
        expiresOn: DateTimeOffset.UtcNow.AddMinutes(5),
        extendedExpiresOn: DateTimeOffset.UtcNow.AddMinutes(5),
        tenantId: "mock-tenant-id",
        account: null,
        idToken: "mock-id-token",
        scopes: ["user.read", "profile"],
        correlationId: Guid.NewGuid(),
        tokenType: "Bearer",
        authenticationResultMetadata: null,
        claimsPrincipal: null,
        spaAuthCode: null,
        additionalResponseParameters: null);

    public Task<string> GetAccessTokenForUserAsync(IEnumerable<string> scopes, string? authenticationScheme, string? tenantId = null,
        string? userFlow = null, ClaimsPrincipal? user = null, TokenAcquisitionOptions? tokenAcquisitionOptions = null)
    {
        if (user?.Claims == null)
            return Task.FromResult(String.Empty);
            
        var claims = user.Claims; // copy or map as needed

        var secret = "use-a-64-byte-minimum-secret-string................................"; // >=64 bytes
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        claims = claims.Select(c =>
        {
            if (c.Type == "aud")
            {
                return new Claim(c.Type, "api://client-id/.default");
            }
            
            return new Claim(c.Type, c.Value);
        });
        
        var jwt = new JwtSecurityToken(
            issuer: "rando-issuer",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512));

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwt));
    }

    public Task<AuthenticationResult> GetAuthenticationResultForUserAsync(IEnumerable<string> scopes,
        string? authenticationScheme, string? tenantId = null,
        string? userFlow = null, ClaimsPrincipal? user = null, TokenAcquisitionOptions? tokenAcquisitionOptions = null)
        => Task.FromResult(MockAuthenticationResult);

    public Task<string> GetAccessTokenForAppAsync(string scope, string? authenticationScheme, string? tenant = null,
        TokenAcquisitionOptions? tokenAcquisitionOptions = null)
        => Task.FromResult("mock-app-access-token");

    public Task<AuthenticationResult> GetAuthenticationResultForAppAsync(string scope, string? authenticationScheme, string? tenant = null,
        TokenAcquisitionOptions? tokenAcquisitionOptions = null)
        => Task.FromResult(MockAuthenticationResult);

    public void ReplyForbiddenWithWwwAuthenticateHeader(IEnumerable<string> scopes, MsalUiRequiredException msalServiceException,
        string? authenticationScheme, HttpResponse? httpResponse = null)
    { }

    public string GetEffectiveAuthenticationScheme(string? authenticationScheme)
        => "MockScheme";

    public Task ReplyForbiddenWithWwwAuthenticateHeaderAsync(
        IEnumerable<string> scopes, 
        MsalUiRequiredException msalServiceException,
        HttpResponse? httpResponse = null) => Task.CompletedTask;
}