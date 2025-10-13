using Common.Enum;

namespace Common.Domain;

public class AnswerDto
{
    public int Id { get; set; }
    public int QuestionnaireId { get; set; }
    public required int QuestionId { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    public string? Destination { get; set; }
    
    public int? DestinationQuestionId { get; set; }
    
    public int? DestinationContentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}