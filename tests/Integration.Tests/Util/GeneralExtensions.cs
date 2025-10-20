using System.Text.Json;
using Common.Enum;

namespace Integration.Tests.Util;

public static class GeneralExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    
    public static TGetToAnAnswerDto? Deserialize<TGetToAnAnswerDto>(this string json)
    {
        return JsonSerializer.Deserialize<TGetToAnAnswerDto>(json, JsonOptions);
    }
    
    public static bool HasOnlyAllowedProps(this JsonElement root, params string[] allowed)
    {
        var allowedSet = new HashSet<string>(allowed, StringComparer.Ordinal);
        
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        foreach (var prop in root.EnumerateObject())
        {
            if (!allowedSet.Contains(prop.Name))
                return false;
        }
        return true;
    }
    
    public static IList<QuestionType> ToList<TEnum>(int repeatCount) where TEnum : struct, Enum
        => Enumerable.Repeat(Enum.GetValues<TEnum>(), repeatCount).SelectMany(x => x).Cast<QuestionType>().ToList();
}