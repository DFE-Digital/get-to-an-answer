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
}