using System.Security.Claims;

namespace Integration.Tests.Fake;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Minimal JWT generator for tests.
// - Supports "none" (no signature) and HS256/HS384/HS512.
// - Produces compact JWTs: header.payload.signature
// - Intended for integration tests; not for production security.

public static class JwtTestTokenGenerator
{
    // algorithm: "none", "HS256", "HS384", "HS512"
    public static string Create(
        string subject,
        string issuer,
        string audience,
        TimeSpan? expiresIn = null,
        DateTimeOffset? expiresAtUtc = null,
        string algorithm = "none",
        string? hmacSecret = null,
        IEnumerable<string>? roles = null,
        IDictionary<string, object>? customClaims = null,
        string? name = null,
        DateTimeOffset? notBeforeUtc = null,
        DateTimeOffset? issuedAtUtc = null)
    {
        var header = new Dictionary<string, object>
        {
            ["typ"] = "JWT",
            ["alg"] = NormalizeAlg(algorithm)
        };

        var now = DateTimeOffset.UtcNow;
        var iat = (issuedAtUtc ?? now).ToUnixTimeSeconds();

        long? exp = null;
        if (expiresAtUtc.HasValue)
        {
            exp = expiresAtUtc.Value.ToUnixTimeSeconds();
        }
        else if (expiresIn.HasValue)
        {
            exp = now.Add(expiresIn.Value).ToUnixTimeSeconds();
        }
        else
        {
            // default validity: 1 hour
            exp = now.AddHours(1).ToUnixTimeSeconds();
        }

        long? nbf = notBeforeUtc?.ToUnixTimeSeconds();

        var payload = new Dictionary<string, object>
        {
            ["sub"] = subject,
            ["iss"] = issuer,
            ["aud"] = audience,
            ["iat"] = iat,
            ["exp"] = exp!,
            [ClaimTypes.Email] = subject + "@example.com",
        };

        if (nbf.HasValue) payload["nbf"] = nbf.Value;
        if (!string.IsNullOrWhiteSpace(name)) payload["name"] = name!;

        if (roles != null)
        {
            var rolesArray = roles.ToArray();
            if (rolesArray.Length == 1)
                payload["role"] = rolesArray[0]; // some APIs expect single role claim
            else if (rolesArray.Length > 1)
                payload["roles"] = rolesArray;   // others expect array
        }

        if (customClaims != null)
        {
            foreach (var kvp in customClaims)
            {
                payload[kvp.Key] = kvp.Value;
            }
        }

        var headerB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header, JsonOptions));
        var payloadB64 = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions));
        var signingInput = $"{headerB64}.{payloadB64}";

        var alg = (string)header["alg"];
        string signatureB64 = alg == "none"
            ? "" // Empty signature segment per RFC when alg is "none"
            : Base64UrlEncode(Sign(signingInput, alg, hmacSecret));

        return $"{signingInput}.{signatureB64}";
    }

    private static byte[] Sign(string signingInput, string alg, string? secret)
    {
        if (string.IsNullOrEmpty(secret))
            throw new ArgumentException("hmacSecret must be provided for HMAC algorithms.", nameof(secret));

        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.ASCII.GetBytes(signingInput);

        return alg switch
        {
            "HS256" => ComputeHmac(HashAlgorithmName.SHA256, key, data),
            "HS384" => ComputeHmac(HashAlgorithmName.SHA384, key, data),
            "HS512" => ComputeHmac(HashAlgorithmName.SHA512, key, data),
            _ => throw new NotSupportedException($"Unsupported algorithm: {alg}. Use 'none', 'HS256', 'HS384', or 'HS512'.")
        };
    }

    private static byte[] ComputeHmac(HashAlgorithmName name, byte[] key, byte[] data)
    {
        using HMAC hmac = name.Name switch
        {
            "SHA256" => new HMACSHA256(key),
            "SHA384" => new HMACSHA384(key),
            "SHA512" => new HMACSHA512(key),
            _ => throw new NotSupportedException($"Unsupported hash: {name}.")
        };
        return hmac.ComputeHash(data);
    }

    private static string NormalizeAlg(string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm)) return "none";
        var alg = algorithm.ToUpperInvariant();
        return alg switch
        {
            "NONE" => "none",
            "HS256" => "HS256",
            "HS384" => "HS384",
            "HS512" => "HS512",
            _ => throw new NotSupportedException($"Unsupported algorithm: {algorithm}. Use 'none', 'HS256', 'HS384', or 'HS512'.")
        };
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        var s = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return s;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    
    private const string Secret = "local-test-signing-key-32bytes-minimum!";
    public static readonly string ValidJwtToken = JwtTestTokenGenerator.Create(
        subject: "user_mock",
        issuer: "http://dfe-issuer",
        audience: "test-audience",
        expiresIn: TimeSpan.FromHours(1),
        algorithm: "HS256", //with hmacSecret for signed tokens
        hmacSecret: Secret
    );
    
    public static readonly string ExpiredJwtToken = JwtTestTokenGenerator.Create(
        subject: "user_mock",
        issuer: "http://dfe-issuer",
        audience: "test-audience",
        expiresAtUtc: DateTimeOffset.UtcNow.AddHours(-1),
        algorithm: "HS256",
        hmacSecret: Secret
    );
    
    public static readonly string InvalidAudJwtToken = JwtTestTokenGenerator.Create(
        subject: "user_mock",
        issuer: "http://dfe-issuer",
        audience: "wrong-audience",
        expiresIn: TimeSpan.FromHours(1),
        algorithm: "HS256",
        hmacSecret: Secret
    );
    
    public static readonly string UnauthorizedJwtToken = JwtTestTokenGenerator.Create(
        subject: "unauthorized_user",
        issuer: "http://dfe-issuer",
        audience: "test-audience",
        expiresIn: TimeSpan.FromHours(1),
        algorithm: "HS256",
        roles: Array.Empty<string>(), // omit roles
        hmacSecret: Secret
    );
    
    public static readonly string NonDfeJwtToken = JwtTestTokenGenerator.Create(
        subject: "unauthorized_user",
        issuer: "http://non-dfe-issuer",
        audience: "test-audience",
        expiresIn: TimeSpan.FromHours(1),
        algorithm: "HS256",
        roles: Array.Empty<string>(), // omit roles
        hmacSecret: Secret
    );
    
    public static readonly string QuotaWarnJwtToken = JwtTestTokenGenerator.Create(
        subject: "user_quota",
        issuer: "http://dfe-issuer",
        audience: "test-audience",
        expiresIn: TimeSpan.FromHours(1),
        algorithm: "HS256",
        customClaims: new Dictionary<string, object> { ["test_warn"] = "over_quota" },
        hmacSecret: Secret
    );
    
    public static readonly string NewUserJwtToken = JwtTestTokenGenerator.Create(
        subject: "new_user" + Guid.NewGuid(),
        issuer: "http://dfe-issuer",
        audience: "test-audience",
        expiresIn: TimeSpan.FromHours(1),
        algorithm: "HS256",
        roles: Array.Empty<string>(), // omit roles
        hmacSecret: Secret
    );
}