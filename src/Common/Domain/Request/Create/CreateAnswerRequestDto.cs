using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Create;

public class CreateAnswerRequestDto
{
    public required Guid QuestionnaireId { get; set; }
    public required Guid QuestionId { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float Score { get; set; }
    
    [EnumDefined]
    public DestinationType? DestinationType { get; set; }
    
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", 
        ErrorMessage = "Please enter a valid URL")]
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
}