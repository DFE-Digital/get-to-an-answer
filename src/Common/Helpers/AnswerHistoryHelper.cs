using System.Text.Json;

namespace Common.Helpers;

/// <summary>
/// Helper class for managing answer history JSON serialization and deserialization.
/// </summary>
public static class AnswerHistoryHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    /// <summary>
    /// Gets the default JSON serializer options for answer history.
    /// </summary>
    public static JsonSerializerOptions Options => DefaultOptions;

    /// <summary>
    /// Saves the selected answers to the answer history for the current question.
    /// </summary>
    public static string SaveAnswersToHistory(string currentHistory, Guid questionId, List<Guid> selectedAnswerIds)
    {
        if (questionId == Guid.Empty || selectedAnswerIds == null || selectedAnswerIds.Count == 0)
            return currentHistory;

        if (string.IsNullOrEmpty(currentHistory))
            currentHistory = "{}";

        var history = JsonSerializer.Deserialize<Dictionary<string, List<Guid>>>(currentHistory, DefaultOptions) ?? new();
        history[questionId.ToString()] = selectedAnswerIds;
        return JsonSerializer.Serialize(history, DefaultOptions);
    }

    /// <summary>
    /// Retrieves previously selected answers for a question from the history.
    /// </summary>
    public static List<Guid> GetAnswersFromHistory(string history, Guid questionId)
    {
        if (questionId == Guid.Empty)
            return new();

        if (string.IsNullOrEmpty(history))
            history = "{}";

        var deserializedHistory = JsonSerializer.Deserialize<Dictionary<string, List<Guid>>>(history, DefaultOptions) ?? new();
        var questionKey = questionId.ToString();
        return deserializedHistory.TryGetValue(questionKey, out var answers) && answers != null ? answers : new();
    }
}
