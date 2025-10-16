using System.Security.Claims;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Integration.Tests.Fake;

public sealed class WireMockAadServer : IDisposable
{
    public WireMockServer Server { get; }

    public string BaseUrl => Server.Urls[0];
    public string TenantId { get; }
    public string Issuer => $"{BaseUrl}/{TenantId}/v2.0";

    public WireMockAadServer(string? tenantId = null, int? port = null)
    {
        TenantId = tenantId ?? "mock-tenant";
        Server = port.HasValue
            ? WireMockServer.Start(port.Value)
            : WireMockServer.Start();

        SetupDiscovery();
        SetupTokenEndpoint();
        SetupUserInfo();
        SetupJwks();
    }

    private void SetupDiscovery()
    {
        var discovery = new
        {
            issuer = Issuer,
            authorization_endpoint = $"{BaseUrl}/{TenantId}/oauth2/v2.0/authorize",
            token_endpoint = $"{BaseUrl}/{TenantId}/oauth2/v2.0/token",
            jwks_uri = $"{BaseUrl}/{TenantId}/discovery/v2.0/keys",
            userinfo_endpoint = $"{BaseUrl}/{TenantId}/openid/userinfo",
            token_endpoint_auth_methods_supported = new[] { "client_secret_post", "client_secret_basic" },
            response_types_supported = new[] { "code", "id_token", "code id_token" }
        };

        Server.Given(Request.Create()
                .WithPath($"/{TenantId}/v2.0/.well-known/openid-configuration")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(discovery));
    }

    private void SetupTokenEndpoint()
    {
        // Return a dummy access_token and id_token. In real tests you may want to sign a JWT.
        var tokenResponse = new
        {
            token_type = "Bearer",
            expires_in = 3600,
            ext_expires_in = 3600,
            access_token = JwtTestTokenGenerator.ValidJwtToken,
            id_token = CreateUnsignedIdToken(iss: Issuer, aud: "your-client-id", upn: "user@contoso.com", oid: "00000000-0000-0000-0000-000000000001")
        };

        Server.Given(Request.Create()
                .WithPath($"/{TenantId}/oauth2/v2.0/token")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(tokenResponse));
    }

    private void SetupUserInfo()
    {
        const string email = ClaimTypes.Email;
        
        var userInfo = new Dictionary<string, object?>
        {
            { "sub", "00000000-0000-0000-0000-000000000001" },
            { "name", "Mock User" },
            { "preferred_username", "user@contoso.com" },
            { "tid", TenantId },
            { email, "user@contoso.com" }
        };

        Server.Given(Request.Create()
                .WithPath($"/{TenantId}/openid/userinfo")
                .UsingGet()
                .WithHeader(headers =>
                {
                    // Perform lightweight JWT checks against the Authorization header
                    if (!headers.TryGetValue("Authorization", out var authHeaderValues))
                        return false;

                    var authHeader = authHeaderValues?.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        return false;

                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    return ValidateTokenShape(token);
                }))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(userInfo));

        // Fallback for invalid token -> 401
        Server.Given(Request.Create()
                .WithPath($"/{TenantId}/openid/userinfo")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(401)
                .WithHeader("WWW-Authenticate", $"Bearer authorization_uri=\"{Issuer}\", error=\"invalid_token\"")
                .WithBody("invalid token"));
    }

    private void SetupJwks()
    {
        // For unsigned or "none" alg tokens, many validators still try to fetch keys.
        // Provide an empty JWKS or a fake key set if your app requires validation.
        var jwks = new
        {
            keys = new object[] { }
        };

        Server.Given(Request.Create()
                .WithPath($"/{TenantId}/discovery/v2.0/keys")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(jwks));
    }

    private static bool ValidateTokenShape(string jwt)
    {
        // Expect 3 parts even if unsigned (ending dot allowed)
        var parts = jwt.Split('.');
        if (parts.Length < 2) return false;

        try
        {
            var headerJson = Base64UrlDecodeToString(parts[0]);
            var payloadJson = Base64UrlDecodeToString(parts[1]);

            using var headerDoc = JsonDocument.Parse(headerJson);
            using var payloadDoc = JsonDocument.Parse(payloadJson);

            // alg should be "none" for our unsigned test tokens or a known alg
            if (headerDoc.RootElement.TryGetProperty("alg", out var algEl))
            {
                var alg = algEl.GetString();
                if (string.IsNullOrWhiteSpace(alg)) return false;
                if (alg is not ("none" or "RS256" or "RS384" or "RS512"))
                    return false;
            }
            else
            {
                return false;
            }

            var p = payloadDoc.RootElement;

            // Required claims: iss, aud, tid, exp
            if (!p.TryGetProperty("iss", out var issEl) || string.IsNullOrWhiteSpace(issEl.GetString()))
                return false;

            if (!p.TryGetProperty("aud", out var audEl) || string.IsNullOrWhiteSpace(audEl.GetString()))
                return false;

            if (!p.TryGetProperty("tid", out var tidEl) || string.IsNullOrWhiteSpace(tidEl.GetString()))
                return false;

            if (!p.TryGetProperty("exp", out var expEl))
                return false;

            // Exp not in the past
            var exp = expEl.GetInt64();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (exp <= now) return false;

            // Example audience/tenant checks used in tests
            var aud = audEl.GetString();
            if (aud != "your-client-id") return false;

            var tid = tidEl.GetString();
            if (tid != "mock-tenant") return false;

            // Email or preferred_username presence (used by API)
            var hasEmail = p.TryGetProperty("email", out var _) || p.TryGetProperty("preferred_username", out var _);
            if (!hasEmail) return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string Base64UrlDecodeToString(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        var bytes = Convert.FromBase64String(s);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    private static string CreateUnsignedIdToken(string iss, string aud, string upn, string oid)
    {
        // Minimal unsigned JWT using "none" alg. Adjust claims per your app.
        // If your middleware enforces signature validation, replace with a signed token and matching JWKS.
        var header = new { alg = "none", typ = "JWT" };
        var payload = new Dictionary<string, object?>
        {
            ["iss"] = iss,
            ["aud"] = aud,
            ["upn"] = upn,
            ["name"] = "Mock User",
            ["oid"] = oid,
            ["tid"] = "mock-tenant",
            ["ver"] = "2.0",
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            ["email"] = "user@contoso.com",
            ["preferred_username"] = "user@contoso.com"
        };

        string B64(object o) => Base64Url(JsonSerializer.Serialize(o));
        return $"{B64(header)}.{B64(payload)}.";
    }

    private static string Base64Url(string s)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(s);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public void Dispose()
    {
        Server.Stop();
        Server.Dispose();
    }
}