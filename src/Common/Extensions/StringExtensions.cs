using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Local;

namespace Common.Extensions;

public static class StringExtensions
{
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
    
    public static string ToHintTextColor(this string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor)) return "#000000";
        
        // Remove # if present
        hexColor = hexColor.TrimStart('#');
        
        // Validate hex color length
        if (hexColor.Length != 6)
        {
            // Handle short hex format (#RGB -> #RRGGBB)
            if (hexColor.Length == 3)
            {
                hexColor = $"{hexColor[0]}{hexColor[0]}{hexColor[1]}{hexColor[1]}{hexColor[2]}{hexColor[2]}";
            }
            else
            {
                // Invalid format, return default
                return "#000000";
            }
        }
        
        // Validate that all characters are valid hex digits
        if (!hexColor.All(c => Uri.IsHexDigit(c)))
        {
            return "#000000";
        }
        
        // Parse hex to RGB
        int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
        int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
        int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
        
        // Calculate the difference between #0b0c0c and #505a5f
        // #0b0c0c = RGB(11, 12, 12)
        // #505a5f = RGB(80, 90, 95)
        // Difference: R+69, G+78, B+83
        
        int newR = Math.Min(r + 69, 255);
        int newG = Math.Min(g + 78, 255);
        int newB = Math.Min(b + 83, 255);
        
        // Convert to hex
        return $"#{newR:X2}{newG:X2}{newB:X2}";
    }
    
    public static string ToHoverColor(this string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor)) return "#000000";
        
        // Remove # if present
        hexColor = hexColor.TrimStart('#');
        
        // Validate hex color length
        if (hexColor.Length != 6)
        {
            // Handle short hex format (#RGB -> #RRGGBB)
            if (hexColor.Length == 3)
            {
                hexColor = $"{hexColor[0]}{hexColor[0]}{hexColor[1]}{hexColor[1]}{hexColor[2]}{hexColor[2]}";
            }
            else
            {
                // Invalid format, return default
                return "#000000";
            }
        }
        
        // Validate that all characters are valid hex digits
        if (!hexColor.All(c => Uri.IsHexDigit(c)))
        {
            return "#000000";
        }
        
        // Parse hex to RGB
        int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
        int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
        int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
        
        // Calculate the difference between #00703c and #005a30
        // #00703c = RGB(0, 112, 60)
        // #005a30 = RGB(0, 90, 48)
        // Difference: R-0, G-22, B-12
        
        int newR = Math.Max(r - 0, 0);
        int newG = Math.Max(g - 22, 0);
        int newB = Math.Max(b - 12, 0);
        
        // Convert to hex
        return $"#{newR:X2}{newG:X2}{newB:X2}";
    }
    
    public static string ToShadowColor(this string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor)) return "#000000";
        
        // Remove # if present
        hexColor = hexColor.TrimStart('#');
        
        // Validate hex color length
        if (hexColor.Length != 6)
        {
            // Handle short hex format (#RGB -> #RRGGBB)
            if (hexColor.Length == 3)
            {
                hexColor = $"{hexColor[0]}{hexColor[0]}{hexColor[1]}{hexColor[1]}{hexColor[2]}{hexColor[2]}";
            }
            else
            {
                // Invalid format, return default
                return "#000000";
            }
        }
        
        // Validate that all characters are valid hex digits
        if (!hexColor.All(c => Uri.IsHexDigit(c)))
        {
            return "#000000";
        }
        
        // Parse hex to RGB
        int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
        int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
        int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
        
        // Calculate the difference between #00703c and #002d18
        // #00703c = RGB(0, 112, 60)
        // #002d18 = RGB(0, 45, 24)
        // Difference: R-0, G-67, B-36
        
        int newR = Math.Max(r - 0, 0);
        int newG = Math.Max(g - 67, 0);
        int newB = Math.Max(b - 36, 0);
        
        // Convert to hex
        return $"#{newR:X2}{newG:X2}{newB:X2}";
    }
}