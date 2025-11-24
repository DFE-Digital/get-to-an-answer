namespace Common.Domain.Admin;

public class QuestionnaireBranchingMapDto
{
    public required Guid QuestionnaireId { get; set; }
    public required string QuestionnaireTitle { get; set; }
    public required string Source { get; set; }
}