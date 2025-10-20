using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateAnswerRequestDto
{
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float? Score { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", 
        ErrorMessage = "Please enter a valid URL")]
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
}