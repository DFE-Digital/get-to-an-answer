namespace Common.Custom;

using System.Text.RegularExpressions;

public static class JsonPathDescriber
{
    // e.g. $.questions[4].answers[3].Content
    private static readonly Regex PathPartRegex =
        new(@"(?<name>[A-Za-z0-9_]+)(\[(?<index>\d+)\])?", RegexOptions.Compiled);

    public static string Describe(string? jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            return string.Empty;
        }

        // Strip leading "$." or "$"
        if (jsonPath.StartsWith("$."))
            jsonPath = jsonPath[2..];
        else if (jsonPath.StartsWith("$"))
            jsonPath = jsonPath[1..];

        var parts = jsonPath.Split('.', StringSplitOptions.RemoveEmptyEntries);

        string? questionPart = null;
        string? answerPart = null;
        string? contentPart = null;
        string? otherPart = null;

        foreach (var part in parts)
        {
            var match = PathPartRegex.Match(part);
            if (!match.Success) continue;

            var name = match.Groups["name"].Value;
            var indexGroup = match.Groups["index"];
            int? index = indexGroup.Success ? int.Parse(indexGroup.Value) : null;

            switch (name.ToLowerInvariant())
            {
                case "questions":
                case "question":
                    if (index is not null)
                    {
                        var questionNumber = index.Value + 1;
                        questionPart = $"question {questionNumber}";
                    }
                    break;

                case "answers":
                case "answer":
                    if (index is not null)
                    {
                        var answerNumber = index.Value + 1;
                        answerPart = $"answer {answerNumber}";
                    }
                    break;

                case "content":
                case "title":
                case "description":
                    contentPart = name.ToLowerInvariant();
                    break;

                default:
                    // Fallback for other properties or collections
                    otherPart = ToFriendlyName(name);
                    break;
            }
        }

        // Build the phrase
        // Preferred pattern examples:
        //  - "the content of answer 4 of question 5"
        //  - "the description of question 2"
        //  - "the title of this questionnaire"
        var segments = new List<string>();

        if (contentPart is not null)
        {
            segments.Add($"the {contentPart}");
        }
        else if (otherPart is not null)
        {
            segments.Add($"the {otherPart}");
        }
        else
        {
            // Last resort: just "this field"
            segments.Add("this field");
        }

        if (answerPart is not null)
        {
            segments.Add($"of {answerPart}");
        }

        if (questionPart is not null)
        {
            segments.Add($"of {questionPart}");
        }

        return string.Join(" ", segments);
    }

    private static string ToFriendlyName(string identifier)
    {
        // Break PascalCase / camelCase to words, then lower-case
        var withSpaces = Regex.Replace(
            identifier,
            "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
            " $1");

        return withSpaces.ToLowerInvariant();
    }
}