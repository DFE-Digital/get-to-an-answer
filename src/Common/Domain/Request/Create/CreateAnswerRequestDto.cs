using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Create;

public class CreateAnswerRequestDto
{
    public required Guid QuestionnaireId { get; set; }
    public required Guid QuestionId { get; set; }
    
    [Required(ErrorMessage = "Enter an answer")]
    [GdsTitle(MinLength = 1, MaxLength = 500,
        ErrorMessage = "Title must be in plain language, avoid all-caps, not be empty or only whitespace, and should not contain repeated spaces.")]
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public float Priority { get; set; }
    
    [EnumDefined]
    public DestinationType? DestinationType { get; set; }
    
    [MaxLength(250)]
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", 
        ErrorMessage = "Please enter a valid URL")]
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
    public Guid? DestinationContentId { get; set; }
}