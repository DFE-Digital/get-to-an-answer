namespace Common.Helpers;

/// <summary>
/// Helper class for managing answer history serialization and deserialization.
/// Format: question GUID (32 hex chars, no dashes) '.' answer GUIDs joined by '_', entries joined by '~'.
/// Example: 3f2504e04f8911d39a0c0305e82c3301.9b1deb4d3b7d4bad9bdd2b0d7b3dcb6d_1a2b3c4d5e6f47a8b9c0d1e2f3a4b5c6
/// </summary>
public static class AnswerHistoryHelper
{
    private const char EntrySeparator = '~';
    private const char QuestionSeparator = '.';
    private const char AnswerSeparator = '_';

    /// <summary>
    /// Saves the selected answers to the answer history for the current question.
    /// </summary>
    public static string SaveAnswersToHistory(string? currentHistory, Guid questionId, List<Guid> selectedAnswerIds)
    {
        if (questionId == Guid.Empty || selectedAnswerIds == null || selectedAnswerIds.Count == 0)
            return currentHistory ?? string.Empty;

        var history = Decode(currentHistory);
        var index = history.Keys.ToList().IndexOf(questionId);
        if (index >= 0)
        {
            foreach (var staleQuestionId in history.Keys.Skip(index).ToList())
                history.Remove(staleQuestionId);
        }

        history[questionId] = selectedAnswerIds;
        return Encode(history);
    }

    /// <summary>
    /// Adds or updates the selected answers for a question without removing other questions' entries.
    /// </summary>
    public static string UpsertAnswers(string? currentSelections, Guid questionId, List<Guid> selectedAnswerIds)
    {
        if (questionId == Guid.Empty || selectedAnswerIds == null || selectedAnswerIds.Count == 0)
            return currentSelections ?? string.Empty;

        var selections = Decode(currentSelections);
        selections[questionId] = new List<Guid>(selectedAnswerIds);
        return Encode(selections);
    }

    /// <summary>
    /// Retrieves the selected answers for a question from the current selected answers.
    /// </summary>
    public static List<Guid> GetSelectedAnswers(string? currentSelections, Guid questionId)
    {
        if (questionId == Guid.Empty)
            return new();

        return Decode(currentSelections).TryGetValue(questionId, out var answers) ? answers : new();
    }

    /// <summary>
    /// Retrieves previously selected answers for a question from the history.
    /// </summary>
    public static List<Guid> GetAnswersFromHistory(string? history, Guid questionId)
    {
        if (questionId == Guid.Empty)
            return new();

        return Decode(history).TryGetValue(questionId, out var answers) ? answers : new();
    }

    /// <summary>
    /// Returns the IDs of answered questions in the order they were answered.
    /// </summary>
    public static List<Guid> GetAnsweredQuestionIds(string? history)
    {
        return Decode(history).Keys.ToList();
    }

    /// <summary>
    /// Returns the question answered immediately before the given question on the recorded path.
    /// If the question is not in the history, the last answered question is returned.
    /// </summary>
    public static Guid? GetPreviousQuestionId(string? history, Guid currentQuestionId)
    {
        var questionIds = Decode(history).Keys.ToList();
        var index = questionIds.IndexOf(currentQuestionId);
        if (index < 0)
            return questionIds.Count > 0 ? questionIds[^1] : null;

        return index > 0 ? questionIds[index - 1] : null;
    }

    private static string Encode(OrderedDictionary<Guid, List<Guid>> history)
    {
        return string.Join(EntrySeparator, history
            .Where(entry => entry.Value.Count > 0)
            .Select(entry => entry.Key.ToString("N") + QuestionSeparator +
                string.Join(AnswerSeparator, entry.Value.Where(id => id != Guid.Empty).Select(id => id.ToString("N")))));
    }

    private static OrderedDictionary<Guid, List<Guid>> Decode(string? history)
    {
        var result = new OrderedDictionary<Guid, List<Guid>>();
        if (string.IsNullOrEmpty(history) || history == "~")
            return result;

        foreach (var entry in history.Split(EntrySeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Split(QuestionSeparator);
            if (parts.Length != 2 || !TryParseGuid(parts[0], out var questionId))
                continue;

            var answers = new List<Guid>();
            foreach (var token in parts[1].Split(AnswerSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (TryParseGuid(token, out var answerId))
                    answers.Add(answerId);
            }

            if (answers.Count > 0)
                result[questionId] = answers;
        }

        return result;
    }

    private static bool TryParseGuid(string token, out Guid id)
    {
        return Guid.TryParseExact(token, "N", out id) && id != Guid.Empty;
    }
}
