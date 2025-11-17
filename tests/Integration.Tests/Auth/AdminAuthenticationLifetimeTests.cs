using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Integration.Tests.Fake;
using Integration.Tests.Fixture;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Integration.Tests.Auth;


public class AdminAuthenticationCookieFlowTests
    : IClassFixture<AdminWithWireMockFixture>
{
    private readonly AdminWithWireMockFixture _fx;
    
    public AdminAuthenticationCookieFlowTests(AdminWithWireMockFixture fx, ITestOutputHelper output)
    {
        _fx = fx;
        fx.EnableXunitLogging(output);
    }
    
    private const string CookieName = ".AspNetCore.Cookies";

    [Fact(Skip = "Test class is ignored")]
    public async Task Protected_Page_No_Cookie_Triggers_Oidc_Challenge()
    {
        var noRedirect = _fx.CreateClientNoRedirects();

        var res = await noRedirect.GetAsync("/");

        res.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = res.Headers.Location?.ToString() ?? string.Empty;
        location.Should().Contain("authorize");
        var parsed = QueryHelpers.ParseQuery(new Uri(location).Query);
        parsed.Keys.Should().Contain(k => k.Equals("redirect_uri", StringComparison.OrdinalIgnoreCase)
                                          || k.Equals("returnUrl", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(Skip = "Test class is ignored")]
    public async Task Expired_Cookie_Principal_Forces_Reauth()
    {
        var noRedirect = _fx.CreateClientNoRedirects();
        var authed = _fx.CreateClientFollowingRedirects();

        var first = await noRedirect.GetAsync("/admin/questionnaires/manage");
        first.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var claims = new Dictionary<string, string>
        {
            [ClaimTypes.NameIdentifier] = "it-user",
            [ClaimTypes.Name] = "Integration Tester",
            [ClaimTypes.Email] = "it@example.com",
            [ClaimTypes.Role] = "Admin"
        };

        // Sign in with a short expiry
        var signInRes = await noRedirect.PostAsJsonAsync("/__test__/signin-cookie", new
        {
            claims,
            expiresInSeconds = 120
        });
        signInRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the cookie was set
        var setCookie = signInRes.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.FirstOrDefault(c => c.StartsWith(CookieName + "=", StringComparison.OrdinalIgnoreCase))
            : null;
        setCookie.Should().NotBeNull();

        authed.DefaultRequestHeaders.Add("Cookie", setCookie!);

        var ok = await authed.GetAsync("/admin/questionnaires/manage");
        ok.StatusCode.Should().Be(HttpStatusCode.OK);

        var expire = await authed.PostAsync("/__test__/expire-cookie", null as HttpContent);
        expire.StatusCode.Should().Be(HttpStatusCode.OK);

        var challenged = await noRedirect.GetAsync("/admin/questionnaires/manage");
        challenged.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var loc = challenged.Headers.Location?.ToString() ?? string.Empty;
        loc.ToLowerInvariant().Should().Contain("authorize");
    }
    
    [Fact(Skip = "Disabled because breaks all the other tests")]
    public async Task Check_UseTokenLifetime_True_Cookie_Expiration_Matches_Token()
    {
        var noRedirect = _fx.CreateClientNoRedirects();

        var first = await noRedirect.GetAsync("/admin/questionnaires/manage");
        first.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var claims = new Dictionary<string, string>
        {
            [ClaimTypes.NameIdentifier] = "it-user",
            [ClaimTypes.Name] = "Integration Tester",
            [ClaimTypes.Email] = "it@example.com",
            [ClaimTypes.Role] = "Admin",
            [ClaimTypes.Expiration] = DateTime.UtcNow.AddDays(1).ToString("o"),
            ["exp"] = DateTime.UtcNow.AddDays(1).ToString("o")
        };

        var days = 20;
        
        // generate a token from the claims
        var token = JwtTestTokenGenerator.Create(
            subject: "user_mock",
            issuer: "http://dfe-issuer",
            audience: "test-audience",
            expiresIn: TimeSpan.FromHours(days),
            algorithm: "HS256", //with hmacSecret for signed tokens
            hmacSecret: JwtTestTokenGenerator.Secret
        );

        noRedirect.DefaultRequestHeaders.Add("Cookie", ".AspNetCore.Correlation.my-xsrf=N");

        // Sign in with a short expiry
        var signInRes = await noRedirect.PostAsync("/__test__/signin-cookie", /*new
        {
            claims,
            expiresInSeconds = -1 // UseTokenLifetime should set the cookie to expire at the same time as the token
        }*/ new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id_token"] = token,
            ["access_token"] = token,
            ["state"] = Convert.ToBase64String(PropertiesSerializer.Default.Serialize(
                new AuthenticationProperties { IsPersistent = true, Items =
                {
                    //
                    { ".xsrf", "my-xsrf" },
                    { OpenIdConnectDefaults.UserstatePropertiesKey, "my-state" },
                    { ".Token.access_token", token }
                } })),
            ["exp"] = DateTime.UtcNow.AddDays(1).ToString("o")
        }));

        // Verify the cookie was set
        var setCookie = signInRes.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.FirstOrDefault(c => c.StartsWith(CookieName + "=", StringComparison.OrdinalIgnoreCase))
            : null;
        setCookie.Should().NotBeNull();
        
        // parse the cookie string
        var cookieParts = setCookie!.Split(';');
        var expiresValue = cookieParts.FirstOrDefault(p => p.Trim().StartsWith("expires=", StringComparison.OrdinalIgnoreCase))
            ?.Split('=')[1].Trim();
        var cookieExpiration = DateTime.Parse(expiresValue!);

        // assert the cookie expires at the same time as the token
        var tokenExpiration = DateTime.Parse(claims[ClaimTypes.Expiration]);
        cookieExpiration.Should().BeCloseTo(tokenExpiration, TimeSpan.FromDays(days));
        
    }
}