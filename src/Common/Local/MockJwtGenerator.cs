using System.Security.Claims;
using System.Text.Json;

namespace Common.Local;

public static class MockJwtGenerator
{
    public static string Create(IDictionary<string, object>? claims = null,
        TimeSpan? validFor = null)
    {
        validFor ??= TimeSpan.FromHours(1);

        var exp = DateTimeOffset.UtcNow.Add(validFor.Value);
        var expUnix = exp.ToUnixTimeSeconds();

        const string expirationClaim = ClaimTypes.Expiration;

        var header = new Dictionary<string, object> { ["alg"] = "none", ["typ"] = "JWT" };
        var payload = new Dictionary<string, object>
        {
            ["exp"] = expUnix,
            [expirationClaim] = exp.ToString("o")
        };

        if (claims != null)
            foreach (var kv in claims)
                payload[kv.Key] = kv.Value;

        var h = Base64Url(JsonSerializer.SerializeToUtf8Bytes(header));
        var p = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        // alg=none -> empty signature segment
        return $"{h}.{p}.";
    }

    private static string Base64Url(byte[] b) =>
        Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}