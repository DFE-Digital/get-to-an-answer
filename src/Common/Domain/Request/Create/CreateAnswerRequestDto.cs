using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Create;

public class CreateAnswerRequestDto
{
    public required int QuestionnaireId { get; set; }
    public required int QuestionId { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    public string? Destination { get; set; }
    
    public int? DestinationQuestionId { get; set; }
} 
