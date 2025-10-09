namespace Common.Domain.Frontend;

public class GetNextStateRequest
{
    public int CurrentQuestionId { get; set; }
    
    public int CurrentQuestionOrder { get; set; }

    public List<int> SelectedAnswerIds { get; set; } = new();
}