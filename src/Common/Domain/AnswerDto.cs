using Common.Enum;

namespace Common.Domain;

public class AnswerDto
{
    public Guid Id { get; set; }
    public Guid QuestionnaireId { get; set; }
    public required Guid QuestionId { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
    
    public Guid? DestinationContentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}