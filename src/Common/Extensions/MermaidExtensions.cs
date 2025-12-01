using Common.Domain;
using Common.Enum;
using Common.Infrastructure.Persistence.Entities;

namespace Common.Extensions;

using System.Text;

// Generates a Mermaid diagram for a QuestionnaireEntity using the template rules:
// - Solid arrows: branching via destination fields
// - Dashed 'contains': Questionnaire has Questions (composition, not traversal order)
// - Answer '(priority: n)': prioritisation only (if priority exists on your Answer entity; omitted here if unavailable)
// - Destinations supported: Question, External Link, Custom Info Page
public static class MermaidExtensions
{
    public static string ToMermaidDiagram(this QuestionnaireEntity questionnaire, Dictionary<Guid, string>? contentMap = null)
    {
        contentMap ??= new Dictionary<Guid, string>();
        
        if (questionnaire == null) throw new ArgumentNullException(nameof(questionnaire));

        var sb = new StringBuilder();
        sb.AppendLine("flowchart TD");
        sb.AppendLine("  %% Generated from Questionnaire -> Questions -> Answers");
        sb.AppendLine("  %% Destinations: Question, External Link, Custom Info Page");
        sb.AppendLine("  %% Priorities are used for routing for multiselect questions");
        sb.AppendLine();

        // Root node
        var qnId = NodeId("QE");
        sb.AppendLine($"  {qnId}[Questionnaire]");
        sb.AppendLine();

        // Map questions to stable IDs
        var questions = questionnaire.Questions?
            .OrderBy(q => q.Order)
            .ToList() ?? new List<QuestionEntity>();

        var questionIds = new Dictionary<Guid, string>();
        for (int i = 0; i < questions.Count; i++)
        {
            var id = NodeId($"Q{i + 1}");
            questionIds[questions[i].Id] = id;
            var label = EscapeLabel(string.IsNullOrWhiteSpace(questions[i].Content) ? $"Question {i + 1}" : Truncate(questions[i].Content!, 60));
            sb.AppendLine($"  {id}{{\"{label}\"}}");
        }

        if (questions.Count > 0)
        {
            // Composition (dashed 'contains') and initial arrow to first question by order
            foreach (var q in questions)
            {
                sb.AppendLine($"  {qnId} -. contains .-> {questionIds[q.Id]}");
            }
            sb.AppendLine($"  {qnId} --> {questionIds[questions[0].Id]}");
        }

        // Containers for special destinations (info pages and external links) to avoid duplicate nodes
        var customInfoNodes = new Dictionary<Guid, string>();   // key: content id or destination string
        var externalLinkNodes = new Dictionary<string, string>(); // key: url

        // Edges from answers based on destinations
        foreach (var q in questions)
        {
            var qNode = questionIds[q.Id];
            var answers = q.Answers?.OrderBy(a => a.Priority).ThenBy(a => a.Id).ToList();
            if (answers == null || answers.Count == 0) continue;

            foreach (var a in answers)
            {
                var answerLabel = BuildAnswerLabel(a.Content, a.Priority); // Priority is optional; if not present, it’s omitted
                switch (a.DestinationType)
                {
                    case DestinationType.Question when a.DestinationQuestionId.HasValue && questionIds.TryGetValue(a.DestinationQuestionId.Value, out var destQNode):
                        sb.AppendLine($"  {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {destQNode}");
                        break;

                    case DestinationType.CustomContent:
                        {
                            if (a.DestinationContentId is { } key)
                            {
                                if (!customInfoNodes.TryGetValue(key, out var infoNodeId))
                                {
                                    infoNodeId = NodeId($"CI{customInfoNodes.Count + 1}");
                                    customInfoNodes[key] = infoNodeId;

                                    var contentTitle = contentMap[key];
                                
                                    var infoLabel = EscapeLabel(string.IsNullOrWhiteSpace(a.DestinationUrl)
                                        ? $"Custom Info '{contentTitle}'"
                                        : Truncate(a.DestinationUrl!, 60));
                                    sb.AppendLine($"  {infoNodeId}[[{infoLabel}]]:::info");
                                }
                                sb.AppendLine($"  {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {infoNodeId}");
                            }
                            
                            break;
                        }

                    case DestinationType.ExternalLink:
                        {
                            var url = string.IsNullOrWhiteSpace(a.DestinationUrl) ? "External Link" : a.DestinationUrl!;
                            if (!externalLinkNodes.TryGetValue(url, out var linkNodeId))
                            {
                                linkNodeId = NodeId($"EL{externalLinkNodes.Count + 1}");
                                externalLinkNodes[url] = linkNodeId;
                                var linkLabel = EscapeLabel(Truncate(url, 60));
                                sb.AppendLine($"  {linkNodeId}{{{{{linkLabel}}}}}:::link");
                            }
                            sb.AppendLine($"  {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {linkNodeId}");
                            break;
                        }

                    default:
                        if (q.Order < questions.Count && questions[q.Order] is { } nextQuestion && 
                            questionIds.TryGetValue(nextQuestion.Id, out var nextQNode))
                        {
                            sb.AppendLine(
                                $"  {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {nextQNode}");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"  {qNode} -- \"{EscapeLabel(answerLabel)}\" --> UNKNOWN([Unknown])");
                        }

                        break;
                }
            }
        }

        // Legend and styles
        sb.AppendLine();
        sb.AppendLine("  %% Legend");
        sb.AppendLine("  subgraph Legend");
        sb.AppendLine("    L1[Solid arrows = branching via destination fields]");
        sb.AppendLine("    L2[Dashed 'contains' = Questionnaire has Questions]");
        sb.AppendLine("    L3[\"Answer '(priority: n)' = prioritisation only\"]");
        sb.AppendLine("    L4[[Custom Info Page]]:::info");
        sb.AppendLine("    L5{{External Link}}:::link");
        sb.AppendLine("  end");
        sb.AppendLine();
        sb.AppendLine("  classDef info fill:#e6f4ff,stroke:#2b7cd3,color:#0b3d91;");
        sb.AppendLine("  classDef link fill:#fff3e6,stroke:#d37c2b,color:#7a3d0b;");

        return sb.ToString();
    }

    // Helpers

    private static string BuildAnswerLabel(string? content, float? priority)
    {
        var baseText = string.IsNullOrWhiteSpace(content) ? "Answer" : Truncate(content!, 60);
        return priority.HasValue ? $"{baseText} (priority: {priority.Value})" : baseText;
    }

    private static string EscapeLabel(string text)
    {
        // Escape quotes and newlines for Mermaid labels
        return text.Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= max) return text;
        return text.Substring(0, Math.Max(0, max - 1)) + "…";
    }

    private static string NodeId(string seed)
    {
        // Mermaid-safe identifier: letters, digits, underscore only
        var sb = new StringBuilder(seed.Length);
        foreach (var ch in seed)
        {
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }
        return sb.ToString();
    }
}