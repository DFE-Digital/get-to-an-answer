namespace Checker.Common.Domain;

public class ConditionDto
{
    public int Id { get; set; }
    
    public int BranchingId { get; set; }
    
    public int QuestionId { get; set; }
    
    public List<int> AnswerIds { get; set; }
}