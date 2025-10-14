namespace Api.Services;

using System.Text.Json;
using Contentful.Core;
using Contentful.Core.Search;
using Microsoft.EntityFrameworkCore;
using Common.Infrastructure.Persistence;
using Common.Infrastructure.Persistence.Entities;
using Common.Enum;

public interface IContentfulSyncService
{
    Task<object?> GetQuestionnaireBySlug(string slug, CancellationToken ct);
    Task HandleWebhook(string topic, string payload, CancellationToken ct);
}

public class ContentfulSyncServiceImpl(ContentfulClient client, GetToAnAnswerDbContext db) : IContentfulSyncService
{
    public async Task<object?> GetQuestionnaireBySlug(string slug, CancellationToken ct)
    {
        var qb = QueryBuilder<dynamic>.New
            .ContentTypeIs("questionnaire")
            .FieldEquals("fields.slug", slug);

        var res = await client.GetEntries(qb, ct);
        var item = res?.FirstOrDefault();
        if (item == null) return null;

        // Expect fields consistent with entities:
        // questionnaire: title, slug, description, status, version, contributors[], createdAtUtc, updatedAtUtc
        // Build a DTO-like anonymous object compatible with your views/APIs.
        var dto = new
        {
            SyncId = (string?)item.sys?.id,
            Title = (string?)item.fields?.title,
            Slug = (string?)item.fields?.slug,
            Description = (string?)item.fields?.description,
            Status = (string?)item.fields?.status,
            Version = (int?)(item.fields?.version ?? 0),
            Contributors = ((IEnumerable<dynamic>?)item.fields?.contributors ?? Enumerable.Empty<dynamic>())
                .Select(c => (string?)c).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray(),
            CreatedAt = TryGetDate(item.fields, "createdAtUtc"),
            UpdatedAt = TryGetDate(item.fields, "updatedAtUtc")
        };

        return dto;
    }

    public async Task HandleWebhook(string topic, string payload, CancellationToken ct)
    {
        var root = JsonDocument.Parse(payload).RootElement;

        if (!root.TryGetProperty("sys", out var sys)) return;
        var contentfulId = sys.GetProperty("id").GetString();
        var contentType = sys.TryGetProperty("contentType", out var ctObj)
            ? ctObj.GetProperty("sys").GetProperty("id").GetString()
            : null;

        switch (topic)
        {
            case { } t when t.EndsWith(".publish"):
            case { } t2 when t2.EndsWith(".auto_save"):
            case { } t3 when t3.EndsWith(".unpublish"):
                await UpsertEntry(contentType, topic switch
                {
                    var top when top.EndsWith(".publish") => EntityStatus.Published, // This is the first switch expression arm
                    var top when top.EndsWith(".unpublish") => EntityStatus.Draft,
                    _ => null,
                }, contentfulId, root, ct);
                break;
            case { } t when t.EndsWith(".delete"):
                await DeleteEntry(contentType, contentfulId, root, ct);
                break;
        }
    }

    private async Task UpsertEntry(string? contentType, EntityStatus? status, string? contentfulId, JsonElement entry, CancellationToken ct)
    {
        if (!entry.TryGetProperty("fields", out var fields)) return;

        // Helper to fetch localized value (first locale)
        string? S(string key) => TryGetLocalizedString(fields, key);
        float Nf(string key) => TryGetLocalizedFloat(fields, key);
        int Ni(string key) => TryGetLocalizedInt(fields, key);
        DateTime? D(string key) => TryGetLocalizedDate(fields, key);

        switch (contentType)
        {
            case "questionnaire":
            {
                var slug = S("slug");
                var existing = await db.Questionnaires
                    .FirstOrDefaultAsync(x => x.SyncId == contentfulId || (!string.IsNullOrWhiteSpace(slug) && x.Slug == slug), ct);

                if (existing == null)
                {
                    var entity = new QuestionnaireEntity
                    {
                        SyncId = contentfulId,
                        Title = S("title") ?? "",
                        Slug = slug,
                        Description = S("description"),
                        Contributors = GetStringArray(fields, "contributors"),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    db.Questionnaires.Add(entity);
                }
                else
                {
                    existing.SyncId = contentfulId ?? existing.SyncId;
                    existing.Title = S("title") ?? existing.Title;
                    existing.Description = S("description");
                    existing.Version += status switch
                    {
                        _ when 
                            existing.Status == EntityStatus.Draft && 
                            status == EntityStatus.Published => 1,
                        _ => 0
                    };
                    existing.Status = status != EntityStatus.Published && 
                                      status != EntityStatus.Draft ? 
                        ParseStatus(S("status")) : existing.Status;
                    existing.Contributors = GetStringArray(fields, "contributors");
                    existing.Slug = slug ?? existing.Slug;
                    existing.UpdatedAt = DateTime.UtcNow;

                    db.Questionnaires.Update(existing);
                }

                await db.SaveChangesAsync(ct);
                break;
            }

            case "question":
            {
                var questionnaireId = await ResolveLinkedLocalIdByContentful(fields, "questionnaire", db, ct);
                if (questionnaireId == null) return;

                // We can key by (questionnaireId + order + content) or store external sys.id on the entity if you add a column.
                var content = S("content") ?? "";
                var order = Ni("order");

                var existing = await db.Questions.FirstOrDefaultAsync(
                    q => q.SyncId == contentfulId || (q.QuestionnaireId == questionnaireId && q.Order == order), ct);

                if (existing == null)
                {
                    var entity = new QuestionEntity
                    {
                        SyncId = contentfulId,
                        QuestionnaireId = questionnaireId.Value,
                        Content = content,
                        Description = S("description"),
                        Type = ParseQuestionType(S("type")),
                        Order = order,
                        Status = ParseStatus(S("status")),
                        CreatedAt = D("createdAtUtc") ?? DateTime.UtcNow,
                        UpdatedAt = D("updatedAtUtc") ?? DateTime.UtcNow
                    };
                    db.Questions.Add(entity);
                }
                else
                {
                    existing.SyncId = contentfulId ?? existing.SyncId;
                    existing.Content = content;
                    existing.Description = S("description");
                    existing.Type = ParseQuestionType(S("type"));
                    existing.Status = ParseStatus(S("status"));
                    existing.UpdatedAt = D("updatedAtUtc") ?? DateTime.UtcNow;

                    db.Questions.Update(existing);
                }

                await db.SaveChangesAsync(ct);
                break;
            }

            case "answer":
            {
                var questionnaireId = await ResolveLinkedLocalIdByContentful(fields, "questionnaire", db, ct);
                var questionId = await ResolveLinkedLocalIdByContentful(fields, "question", db, ct);
                if (questionnaireId == null || questionId == null) return;

                var content = S("content") ?? "";
                var existing = await db.Answers.FirstOrDefaultAsync(
                    a => a.SyncId == contentfulId || (a.QuestionId == questionId && a.Content == content), ct);

                var destinationType = ParseDestinationType(S("destinationType"));
                var destQuestionId = await ResolveLinkedLocalIdByContentful(fields, "destinationQuestion", db, ct);
                var destContentId = await ResolveLinkedLocalIdByContentful(fields, "destinationContent", db, ct);

                if (existing == null)
                {
                    var entity = new AnswerEntity
                    {
                        SyncId = contentfulId,
                        QuestionnaireId = questionnaireId.Value,
                        QuestionId = questionId.Value,
                        Content = content,
                        Description = S("description"),
                        Score = Nf("score"),
                        DestinationType = destinationType,
                        Destination = S("destination"),
                        DestinationQuestionId = destQuestionId,
                        DestinationContentId = destContentId,
                        CreatedAt = D("createdAtUtc") ?? DateTime.UtcNow,
                        UpdatedAt = D("updatedAtUtc") ?? DateTime.UtcNow
                    };
                    db.Answers.Add(entity);
                }
                else
                {
                    existing.SyncId = contentfulId ?? existing.SyncId;
                    existing.Description = S("description");
                    existing.Score = Nf("score");
                    existing.DestinationType = destinationType;
                    existing.Destination = S("destination");
                    existing.DestinationQuestionId = destQuestionId;
                    existing.DestinationContentId = destContentId;
                    existing.UpdatedAt = D("updatedAtUtc") ?? DateTime.UtcNow;

                    db.Answers.Update(existing);
                }

                await db.SaveChangesAsync(ct);
                break;
            }

            case "content":
            {
                var questionnaireId = await ResolveLinkedLocalIdByContentful(fields, "questionnaire", db, ct);
                if (questionnaireId == null) return;

                var title = S("title") ?? "";
                // Prefer RichText; fallback to Text if your model differs
                var body = S("content") ?? "";

                var existing = await db.Contents.FirstOrDefaultAsync(
                    c => c.SyncId == contentfulId || (c.QuestionnaireId == questionnaireId && c.Title == title), ct);

                if (existing == null)
                {
                    var entity = new ContentEntity
                    {
                        SyncId = contentfulId,
                        QuestionnaireId = questionnaireId.Value,
                        Title = title,
                        Content = body,
                        CreatedAt = D("createdAtUtc") ?? DateTime.UtcNow,
                        UpdatedAt = D("updatedAtUtc") ?? DateTime.UtcNow
                    };
                    db.Contents.Add(entity);
                }
                else
                {
                    existing.SyncId = contentfulId ?? existing.SyncId;
                    existing.Content = body;
                    existing.UpdatedAt = D("updatedAtUtc") ?? DateTime.UtcNow;
                    db.Contents.Update(existing);
                }

                await db.SaveChangesAsync(ct);
                break;
            }

            case "questionnaireVersion":
            {
                var questionnaireId = await ResolveLinkedLocalIdByContentful(fields, "questionnaire", db, ct);
                if (questionnaireId == null) return;

                var version = Ni("version");
                var json = S("questionnaireJson") ?? "{}";

                var existing = await db.QuestionnaireVersions.FirstOrDefaultAsync(
                    v => v.SyncId == contentfulId || (v.QuestionnaireId == questionnaireId && v.Version == version), ct);

                if (existing == null)
                {
                    var entity = new QuestionnaireVersionEntity
                    {
                        SyncId = contentfulId,
                        QuestionnaireId = questionnaireId.Value,
                        Version = version,
                        QuestionnaireJson = json,
                        CreatedAt = D("createdAtUtc") ?? DateTime.UtcNow
                    };
                    db.QuestionnaireVersions.Add(entity);
                }
                else
                {
                    existing.SyncId = contentfulId ?? existing.SyncId;
                    existing.QuestionnaireJson = json;
                    db.QuestionnaireVersions.Update(existing);
                }

                await db.SaveChangesAsync(ct);
                break;
            }
        }
    }

    private async Task DeleteEntry(string? contentType, string? contentfulId, JsonElement entry, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(contentfulId)) return;

        switch (contentType)
        {
            case "questionnaire":
            {
                var existing = await db.Questionnaires.FirstOrDefaultAsync(x => x.SyncId == contentfulId, ct);
                if (existing != null)
                {
                    existing.Status = EntityStatus.Deleted;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(ct);
                }
                break;
            }
            case "question":
            {
                var existing = await db.Questions.FirstOrDefaultAsync(x => x.SyncId == contentfulId, ct);
                if (existing != null)
                {
                    db.Questions.Remove(existing);
                    await db.SaveChangesAsync(ct);
                }
                break;
            }
            case "answer":
            {
                var existing = await db.Answers.FirstOrDefaultAsync(x => x.SyncId == contentfulId, ct);
                if (existing != null)
                {
                    db.Answers.Remove(existing);
                    await db.SaveChangesAsync(ct);
                }
                break;
            }
            case "content":
            {
                var existing = await db.Contents.FirstOrDefaultAsync(x => x.SyncId == contentfulId, ct);
                if (existing != null)
                {
                    db.Contents.Remove(existing);
                    await db.SaveChangesAsync(ct);
                }
                break;
            }
            case "questionnaireVersion":
            {
                var existing = await db.QuestionnaireVersions.FirstOrDefaultAsync(x => x.SyncId == contentfulId, ct);
                if (existing != null)
                {
                    db.QuestionnaireVersions.Remove(existing);
                    await db.SaveChangesAsync(ct);
                }
                break;
            }
        }
    }

    // Helpers (same as before, plus link resolver using SyncId inside linked sys)

    private static string? TryGetLocalizedString(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var n)) return null;
        foreach (var prop in n.EnumerateObject())
            return prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.ToString();
        return null;
    }

    private static float TryGetLocalizedFloat(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var n)) return 0f;
        foreach (var prop in n.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetSingle(out var f)) return f;
            if (prop.Value.ValueKind == JsonValueKind.String && float.TryParse(prop.Value.GetString(), out var s)) return s;
        }
        return 0f;
    }

    private static int TryGetLocalizedInt(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var n)) return 0;
        foreach (var prop in n.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var i)) return i;
            if (prop.Value.ValueKind == JsonValueKind.String && int.TryParse(prop.Value.GetString(), out var s)) return s;
        }
        return 0;
    }

    private static DateTime? TryGetDate(dynamic? fields, string key)
    {
        try
        {
            if (fields == null) return null;
            var node = fields?.GetType().GetProperty(key)?.GetValue(fields, null);
            if (node == null) return null;
            var str = node?.ToString() as string;
            return DateTime.TryParse(str, out var dt) ? dt : null;
        }
        catch { return null; }
    }

    private static DateTime? TryGetLocalizedDate(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var n)) return null;
        foreach (var prop in n.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.String && DateTime.TryParse(prop.Value.GetString(), out var dt))
                return dt;
        }
        return null;
    }

    private static EntityStatus ParseStatus(string? s) =>
        s switch
        {
            "Draft" => EntityStatus.Draft,
            "Published" => EntityStatus.Published,
            "Deleted" => EntityStatus.Deleted,
            _ => EntityStatus.Draft
        };

    private static QuestionType ParseQuestionType(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            Enum.TryParse(s, true, out QuestionType type);
            return type;
        }
        return Enum.TryParse<QuestionType>(s, true, out var t) ? t : QuestionType.SingleSelect;
    }

    private static DestinationType? ParseDestinationType(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return Enum.TryParse<DestinationType>(s, true, out var t) ? t : null;
    }

    private static List<string> GetStringArray(JsonElement fields, string name)
    {
        var list = new List<string>();
        if (!fields.TryGetProperty(name, out var n)) return list;
        foreach (var loc in n.EnumerateObject())
        {
            if (loc.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var v in loc.Value.EnumerateArray())
                    if (v.ValueKind == JsonValueKind.String) list.Add(v.GetString()!);
            }
        }
        return list;
    }

    private static async Task<int?> ResolveLinkedLocalIdByContentful(JsonElement fields, string linkField, GetToAnAnswerDbContext db, CancellationToken ct)
    {
        if (!fields.TryGetProperty(linkField, out var n)) return null;

        foreach (var loc in n.EnumerateObject())
        {
            var val = loc.Value;
            if (val.ValueKind != JsonValueKind.Object) continue;
            if (!val.TryGetProperty("sys", out var sys)) continue;
            var id = sys.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            var linkType = sys.TryGetProperty("linkType", out var ltProp) ? ltProp.GetString() : null;

            if (string.IsNullOrWhiteSpace(id) || linkType != "Entry") continue;

            // Probe each table that could be linked
            // questionnaire
            var qn = await db.Questionnaires.Where(x => x.SyncId == id).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
            if (qn != null) return qn;

            // question
            var qu = await db.Questions.Where(x => x.SyncId == id).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
            if (qu != null) return qu;

            // content
            var cn = await db.Contents.Where(x => x.SyncId == id).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);
            if (cn != null) return cn;
        }

        return null;
    }
}