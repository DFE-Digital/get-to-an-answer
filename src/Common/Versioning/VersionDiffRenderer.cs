using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Common.Local;
using Common.Versioning;

namespace Common.Local;

public static class VersionDiffRenderer
{
    public static ChangeMap? RenderCompare(string? fullOldJson, string? fullNewJson)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        
        static string LimitToQuestionnaireContentJson(string? json, JsonSerializerOptions opts)
        {
            if (json == null)
                return string.Empty;
            
            // Deserialize to the known type (ignores unknown fields) and serialize back.
            // Replace with a custom Utf8JsonReader/Writer filter if you want pure streaming.
            var model = JsonSerializer.Deserialize<QuestionnaireContent>(json, opts);
            return JsonSerializer.Serialize(model, opts);
        }

        var oldJson = LimitToQuestionnaireContentJson(fullOldJson, options);
        var newJson = LimitToQuestionnaireContentJson(fullNewJson, options);
        
        var oldNode = JsonNode.Parse(oldJson);
        var newNode = JsonNode.Parse(newJson);

        return BuildChangeMap(oldNode, newNode);
    }

    public static string ToJson(this ICollection<ChangeData?> map)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        
        return JsonSerializer.Serialize(map, options);
    }
    
    public static ChangeMap FilterChangesForSide(this ChangeMap all, bool forNewSide)
    {
        var filtered = new ChangeMap();
        foreach (var (key, value) in all)
        {
            var include = value?.Kind == ChangeKind.Modified
                          || (forNewSide && value?.Kind == ChangeKind.Added)
                          || (!forNewSide && value?.Kind == ChangeKind.Removed);
            if (include)
            {
                filtered[key] = new ChangeData
                {
                    Path = value?.Path,
                    ThisValue = value?.ThisValue,
                    ThatValue = value?.ThatValue,
                    Kind = value?.Kind switch
                    {
                        ChangeKind.Added when forNewSide => ChangeKind.Added,
                        ChangeKind.Removed when !forNewSide => ChangeKind.Removed,
                        ChangeKind.Modified => ChangeKind.Modified,
                        _ => ChangeKind.Unchanged
                    }
                };
            }
        }
        return filtered;
    }
    
    static ChangeMap BuildChangeMap(JsonNode? oldN, JsonNode? newN)
    {
        var map = new ChangeMap();
        Walk("$", "", oldN, newN, map);
        return map;

        static void Walk(string path, object terminalNode, JsonNode? a, JsonNode? b, ChangeMap m)
        {
            if (a is null && b is null) 
                return;

            if (a is null)
            {
                MarkAll(b, a, path, terminalNode, ChangeKind.Added, m); 
                return;
            }

            if (b is null)
            {
                MarkAll(a, b, path, terminalNode, ChangeKind.Removed, m); 
                return;
            }

            if (a is JsonValue && b is JsonValue)
            {
                var av = a.ToJsonString();
                var bv = b.ToJsonString();
                if (av != bv)
                {
                    m[path] = new ChangeData
                    {
                        Kind = ChangeKind.Modified,
                        Path = path,
                        Field = terminalNode is string node ? node : null,
                        Index = terminalNode is int i ? i : null,
                        ThisValue = a,
                        ThatValue = b
                    };
                }

                return;
            }

            if (a is JsonArray aa && b is JsonArray bb)
            {
                var max = Math.Max(aa.Count, bb.Count);
                for (int i = 0; i < max; i++)
                    Walk($"{path}[{i}]", i, i < aa.Count ? aa[i] : null, i < bb.Count ? bb[i] : null, m);
                return;
            }

            if (a is JsonObject ao && b is JsonObject bo)
            {
                var keys = ao.Select(k => k.Key).Union(bo.Select(k => k.Key));
                foreach (var k in keys)
                {
                    ao.TryGetPropertyValue(k, out var av);
                    bo.TryGetPropertyValue(k, out var bv);
                    Walk($"{path}.{k}", k, av, bv, m);
                }
                return;
            }

            m[path] = new ChangeData
            {
                Path = path,
                Kind = ChangeKind.Modified,
                Field = terminalNode is string s ? s : null,
                Index = terminalNode is int node1 ? node1 : null,
                ThisValue = a,
                ThatValue = b
            };
        }

        static void MarkAll(JsonNode? thisNode, JsonNode? thatNode, string path, object terminalNode, ChangeKind kind, ChangeMap m)
        {
            if (thisNode is JsonValue)
            {
                m[path] = new ChangeData
                {
                    Kind = kind, 
                    Path = path,
                    Field = terminalNode is string node ? node : null,
                    Index = terminalNode is int i ? i : null,
                    ThisValue = thisNode,
                    ThatValue = thatNode
                }; 
                return;
            }
            if (thisNode is JsonArray arr)
            {
                var thatArr = thatNode as JsonArray;
                
                for (int i = 0; i < arr.Count; i++)
                    MarkAll(arr[i], thatArr?[i], $"{path}[{i}]", i, kind, m);
                return;
            }
            if (thisNode is JsonObject obj)
            {
                var thatObj = thatNode as JsonObject;
                
                foreach (var (key, value) in obj)
                    MarkAll(value, thatObj?[key],$"{path}.{key}", key, kind, m);
            }
        }
    }
}

public enum ChangeKind { Unchanged, Added, Removed, Modified }

public sealed class ChangeData
{
    public ChangeKind Kind { get; set; }
    public string? Path { get; set; }
    public string? Field { get; set; }
    public int? Index { get; set; }
    public JsonNode? ThisValue { get; set; }
    public JsonNode? ThatValue { get; set; }
}

public sealed class ChangeMap : Dictionary<string, ChangeData?> { } // keys like $.questions[3].title