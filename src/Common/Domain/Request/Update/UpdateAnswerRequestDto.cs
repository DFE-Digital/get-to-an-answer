using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateAnswerRequestDto
{
    public int? Id { get; set; }
    public required int QuestionnaireId { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    public string? Destination { get; set; }
    
    public int? DestinationQuestionId { get; set; }
} 
