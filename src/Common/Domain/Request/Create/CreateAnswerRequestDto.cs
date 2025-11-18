using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Create;

public class CreateAnswerRequestDto
{
    public required Guid QuestionnaireId { get; set; }
    public required Guid QuestionId { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float Priority { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
    
    public Guid? DestinationContentId { get; set; }
} 
