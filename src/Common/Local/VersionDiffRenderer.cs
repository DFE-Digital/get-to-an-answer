using System.Text.Json;
using System.Text.Json.Nodes;
using Common.Local;

namespace Common.Local;

public static class VersionDiffRenderer
{
    public static (string OldHtml, string NewHtml) RenderCompare(string? oldJson, string? newJson)
    {
        if (oldJson == null || newJson == null) 
            return (string.Empty, string.Empty);
        
        var oldNode = JsonNode.Parse(oldJson);
        var newNode = JsonNode.Parse(newJson);

        var changes = BuildChangeMap(oldNode, newNode);

        // Old: highlight Modified + Removed
        var oldHtml = RenderJsonWithHighlights(oldNode, FilterChangesForSide(changes, forNewSide: false));
        // New: highlight Modified + Added
        var newHtml = RenderJsonWithHighlights(newNode, FilterChangesForSide(changes, forNewSide: true));

        return (oldHtml, newHtml);
    }
    
    private static ChangeMap FilterChangesForSide(ChangeMap all, bool forNewSide)
    {
        var filtered = new ChangeMap();
        foreach (var kv in all)
        {
            var include = kv.Value == ChangeKind.Modified
                          || (forNewSide && kv.Value == ChangeKind.Added)
                          || (!forNewSide && kv.Value == ChangeKind.Removed);
            if (include) filtered[kv.Key] = kv.Value switch
            {
                ChangeKind.Added when forNewSide => ChangeKind.Added,
                ChangeKind.Removed when !forNewSide => ChangeKind.Removed,
                ChangeKind.Modified => ChangeKind.Modified,
                _ => ChangeKind.Unchanged
            };
        }
        return filtered;
    }
    
    static ChangeMap BuildChangeMap(JsonNode? oldN, JsonNode? newN)
    {
        var map = new ChangeMap();
        Walk("$", oldN, newN, map);
        return map;

        static void Walk(string path, JsonNode? a, JsonNode? b, ChangeMap m)
        {
            if (a is null && b is null) return;
            if (a is null) { MarkAll(b, path, ChangeKind.Added, m); return; }
            if (b is null) { MarkAll(a, path, ChangeKind.Removed, m); return; }

            if (a is JsonValue && b is JsonValue)
            {
                var av = a.ToJsonString();
                var bv = b.ToJsonString();
                if (av != bv) m[path] = ChangeKind.Modified;
                return;
            }

            if (a is JsonArray aa && b is JsonArray bb)
            {
                var max = Math.Max(aa.Count, bb.Count);
                for (int i = 0; i < max; i++)
                    Walk($"{path}[{i}]", i < aa.Count ? aa[i] : null, i < bb.Count ? bb[i] : null, m);
                return;
            }

            if (a is JsonObject ao && b is JsonObject bo)
            {
                var keys = ao.Select(k => k.Key).Union(bo.Select(k => k.Key));
                foreach (var k in keys)
                {
                    ao.TryGetPropertyValue(k, out var av);
                    bo.TryGetPropertyValue(k, out var bv);
                    Walk($"{path}.{k}", av, bv, m);
                }
                return;
            }

            m[path] = ChangeKind.Modified;
        }

        static void MarkAll(JsonNode node, string path, ChangeKind kind, ChangeMap m)
        {
            if (node is JsonValue) { m[path] = kind; return; }
            if (node is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                    MarkAll(arr[i]!, $"{path}[{i}]", kind, m);
                return;
            }
            if (node is JsonObject obj)
            {
                foreach (var kv in obj)
                    MarkAll(kv.Value!, $"{path}.{kv.Key}", kind, m);
            }
        }
    }
    
    static string RenderJsonWithHighlights(JsonNode? node, ChangeMap changes)  
    {
        var sb = new System.Text.StringBuilder();
        Render("$", node, 0, sb, changes);
        return sb.ToString();

        static void Render(string path, JsonNode? n, int indent, System.Text.StringBuilder sb, ChangeMap changes)
    {
        string Ind(int i) => new string(' ', i * 2);

        if (n is null) { sb.Append("null"); return; }

        if (n is JsonValue v)
        {
            var lit = v.ToJsonString(); // includes quotes for strings
            if (changes.TryGetValue(path, out var kind) && kind != ChangeKind.Unchanged)
                sb.Append(Span(kind, System.Net.WebUtility.HtmlEncode(lit)));
            else
                sb.Append(System.Net.WebUtility.HtmlEncode(lit));
            return;
        }

        if (n is JsonArray arr)
        {
            sb.Append("[");
            if (arr.Count > 0) sb.AppendLine();
            for (int i = 0; i < arr.Count; i++)
            {
                sb.Append(Ind(indent + 1));
                Render($"{path}[{i}]", arr[i], indent + 1, sb, changes);
                if (i < arr.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            if (arr.Count > 0) sb.Append(Ind(indent));
            sb.Append("]");
            return;
        }

        if (n is JsonObject obj)
        {
            sb.Append("{");
            var props = obj.ToArray(); // preserves order of current node
            if (props.Length > 0) sb.AppendLine();
            for (int i = 0; i < props.Length; i++)
            {
                var (key, val) = (props[i].Key, props[i].Value);
                sb.Append(Ind(indent + 1));
                sb.Append(System.Net.WebUtility.HtmlEncode(JsonSerializer.Serialize(key)));
                sb.Append(": ");
                Render($"{path}.{key}", val, indent + 1, sb, changes);
                if (i < props.Length - 1) sb.Append(",");
                sb.AppendLine();
            }
            if (props.Length > 0) sb.Append(Ind(indent));
            sb.Append("}");
            return;
        }
    }

        static string Span(ChangeKind k, string encodedLiteral)
            => k switch
            {
                ChangeKind.Added => $"<span class=\"diff-added\" aria-label=\"Added\">{encodedLiteral}</span>",
                ChangeKind.Removed => $"<span class=\"diff-removed\" aria-label=\"Removed\">{encodedLiteral}</span>",
                _ => $"<span class=\"diff-modified\" aria-label=\"Changed\">{encodedLiteral}</span>"
            };
        }
}

public enum ChangeKind { Unchanged, Added, Removed, Modified }

public sealed class ChangeMap : Dictionary<string, ChangeKind> { } // keys like $.questions[3].title