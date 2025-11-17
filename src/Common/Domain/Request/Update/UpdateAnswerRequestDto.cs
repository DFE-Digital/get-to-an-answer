using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class UpdateAnswerRequestDto
{
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float? Priority { get; set; }
    
    [EnumDefined]
    public DestinationType? DestinationType { get; set; }
    
    [MaxLength(250)]
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", 
        ErrorMessage = "Please enter a valid URL")]
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
    public Guid? DestinationContentId { get; set; }
}