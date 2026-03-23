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
        sb.AppendLine("flowchart LR");
        sb.AppendLine("  subgraph Q [ ]");
        sb.AppendLine("    direction TD");
        
        sb.AppendLine("    %% Generated from Questionnaire -> Questions -> Answers");
        sb.AppendLine("    %% Destinations: Question, External Link, Results Page");
        sb.AppendLine("    %% Priorities are used for routing for multiselect questions");
        sb.AppendLine();

        // Root node
        var qnId = NodeId("QE");
        sb.AppendLine($"    {qnId}({questionnaire.Title}):::questionnaire -->");
        sb.AppendLine();
        
        // Start page, if we have one
        if (!string.IsNullOrEmpty(questionnaire.DisplayTitle))
        {
            var spId = NodeId("SP");
            sb.AppendLine($"    {spId}{{{{{questionnaire.DisplayTitle}}}}}:::startpage -->");
            sb.AppendLine();
        }

        // Map questions to stable IDs
        var questions = questionnaire.Questions?
            .OrderBy(q => q.Order)
            .ToList() ?? new List<QuestionEntity>();

        var questionIds = new Dictionary<Guid, string>();
        for (int i = 0; i < questions.Count; i++)
        {
            var id = NodeId($"Q{i + 1}");
            questionIds[questions[i].Id] = id;
            var label = string.Empty;
            if (!string.IsNullOrWhiteSpace(questions[i].ReferenceName))
            {
                label += $"**Ref: {EscapeLabel(questions[i].ReferenceName!)}**<br/>";
            }

            if (!string.IsNullOrWhiteSpace(questions[i].Content))
            {
                label += EscapeLabel(Truncate(questions[i].Content, 60));
            }
            else
            {
                label += EscapeLabel($"Question {i + 1}");
            }
            
            sb.AppendLine($"    {id}{{\"`{label}`\"}}");
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
                        sb.AppendLine($"    {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {destQNode}");
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
                                        ? $"Results page '{contentTitle}'"
                                        : Truncate(a.DestinationUrl!, 60));
                                    sb.AppendLine($"    {infoNodeId}[[{infoLabel}]]:::result");
                                }
                                sb.AppendLine($"    {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {infoNodeId}");
                            }

                            break;
                        }
                    case DestinationType.InterimThenQuestion:
                    {
                        if (a.DestinationContentId is { } key)
                        {
                            if (!customInfoNodes.TryGetValue(key, out var infoNodeId))
                            {
                                infoNodeId = NodeId($"CI{customInfoNodes.Count + 1}");
                                customInfoNodes[key] = infoNodeId;

                                var contentTitle = contentMap[key];
                                
                                var infoLabel = EscapeLabel(string.IsNullOrWhiteSpace(a.DestinationUrl)
                                    ? $"Interim page '{contentTitle}'"
                                    : Truncate(a.DestinationUrl!, 60));
                                sb.AppendLine($"    {infoNodeId}[{infoLabel}]:::interim");
                            }
                            sb.AppendLine($"    {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {infoNodeId}");
                                
                            if (a.DestinationQuestionId.HasValue && questionIds.TryGetValue(a.DestinationQuestionId.Value, out var destQNode))
                            {
                                sb.AppendLine($"    {infoNodeId} --> {destQNode}");
                            }
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
                                sb.AppendLine($"    {linkNodeId}{{{{{linkLabel}}}}}:::link");
                            }
                            sb.AppendLine($"    {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {linkNodeId}");
                            break;
                        }

                    default:
                        if (q.Order < questions.Count && questions[q.Order] is { } nextQuestion && 
                            questionIds.TryGetValue(nextQuestion.Id, out var nextQNode))
                        {
                            sb.AppendLine(
                                $"    {qNode} -- \"{EscapeLabel(answerLabel)}\" --> {nextQNode}");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"    {qNode} -- \"{EscapeLabel(answerLabel)}\" --> UNKNOWN([Unknown])");
                        }

                        break;
                }
            }
        }
        
        // End the first questionnaire subflow
        sb.AppendLine("  end");

        // Key and styles
        sb.AppendLine();
        sb.AppendLine("  %% Legend");
        sb.AppendLine("  subgraph Legend");
        sb.AppendLine("    direction LR");
        sb.AppendLine("    L1(\"`Solid arrows = branching between questions and answers");
        sb.AppendLine("    <br>");
        sb.AppendLine("    Dashed arrows = all questions");
        sb.AppendLine("    <br>");
        sb.AppendLine("    Answer '(priority: n)' = what priority it is`\")");
        sb.AppendLine("    L2(Questionnaire):::questionnaire");
        sb.AppendLine("    L3{{Start Page}}:::startpage");
        sb.AppendLine("    L4{Question}");
        sb.AppendLine("    L5[Interim Page]:::interim");
        sb.AppendLine("    L6[[Results Page]]:::result");
        sb.AppendLine("    L7{{Link}}:::link");
        sb.AppendLine("  end");
        sb.AppendLine();
        
        // Make sure the legend is on the right of the questionnaire
        sb.AppendLine("  Q ~~~ Legend");
        sb.AppendLine();
        
        // Formatting and styling
        sb.AppendLine("  classDef result fill:#e6f4ff,stroke:#2b7cd3,color:#0b3d91;");
        sb.AppendLine("  classDef link fill:#fff3e6,stroke:#d37c2b,color:#7a3d0b;");
        sb.AppendLine("  classDef interim fill:#fcaee9,stroke:#710456,color:#540234;");
        sb.AppendLine("  classDef questionnaire fill:#b3eba4,stroke:#34861d,color:#173b0d;");
        sb.AppendLine("  classDef startpage fill:#b3eba4,stroke:#34861d,color:#173b0d;");
        sb.AppendLine("  style Q fill:none, stroke: none");

        return sb.ToString();
    }

    // Helpers

    private static string BuildAnswerLabel(string? content, float? priority)
    {
        var baseText = string.IsNullOrWhiteSpace(content) ? "Answer" : Truncate(content!, 60);
        return priority.GetValueOrDefault(0) > 0 ? $"{baseText}<br/>(priority: {priority.Value})" : baseText;
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
