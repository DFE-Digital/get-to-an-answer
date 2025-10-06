using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Create;

public class CreateAnswerRequestDto
{
    public required int QuestionId { get; set; }
    public required string Content { get; set; }
    public required string Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType DestinationType { get; set; }
    
    public string Destination { get; set; }
} 
