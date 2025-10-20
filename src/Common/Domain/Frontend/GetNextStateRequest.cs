namespace Common.Domain.Frontend;

public class GetNextStateRequest
{
    public Guid CurrentQuestionId { get; set; }
    
    public int CurrentQuestionOrder { get; set; }

    public Guid SelectedAnswerId { get; set; }

    public List<Guid> SelectedAnswerIds { get; set; } = new();
}