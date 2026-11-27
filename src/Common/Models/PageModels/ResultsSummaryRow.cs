namespace Common.Models.PageModels;

public record ResultsSummaryRow(Guid QuestionId, string Question, IReadOnlyList<string> Answers);
