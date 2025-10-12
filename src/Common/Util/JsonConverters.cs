namespace Common.Util;

using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// Reusable JSON converter for any T
public static class JsonConverters
{
    public static ValueConverter<T, string> GetJsonConverter<T>(JsonSerializerOptions? options = null) =>
        new ValueConverter<T, string>(
            v => JsonSerializer.Serialize(v, options ?? DefaultOptions),
            v => JsonSerializer.Deserialize<T>(v, options ?? DefaultOptions)!);

    public static ValueComparer<T> GetJsonValueComparer<T>() =>
        new ValueComparer<T>(
            (l, r) => JsonSerializer.Serialize(l, DefaultOptions) == JsonSerializer.Serialize(r, DefaultOptions),
            v => JsonSerializer.Serialize(v, DefaultOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, DefaultOptions), DefaultOptions)!);

    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}