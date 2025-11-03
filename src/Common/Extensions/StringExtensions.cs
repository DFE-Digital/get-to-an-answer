using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Local;

namespace Common.Extensions;

public static class StringExtensions
{
    public static string ToDisplayName(this string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return string.Empty;

        var at = email.IndexOf('@');
        var local = at > 0 ? email[..at] : email;

        // Strip aliases like +team
        var plus = local.IndexOf('+');
        if (plus > -1)
        {
            local = local[..plus];
        }

        // Split on common separators
        var parts = local
            .Split(new[] { '.', '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 0) return local;

        // Title-case each token
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i].ToLowerInvariant();
            parts[i] = char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p[1..] : string.Empty);
        }

        return string.Join(' ', parts);
    }

    public static List<ChangeData> ToChangeDataList(this string? log)
    {
        if (log == null) return [];
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        
        return JsonSerializer.Deserialize<List<ChangeData>>(log, options) ?? [];
    }
}