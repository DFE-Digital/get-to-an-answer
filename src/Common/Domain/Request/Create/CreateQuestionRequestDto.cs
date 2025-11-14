using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Create;

public class CreateQuestionRequestDto
{
    public required Guid QuestionnaireId { get; set; }
    
    [Required(ErrorMessage = "Enter question content")]
    [GdsHeadContent(MinLength = 1, MaxLength = 500,
        ErrorMessage = "Content must be in plain language, avoid all-caps, not be empty or only whitespace, and should not contain repeated spaces.")]
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    [EnumDefined]
    public required QuestionType Type { get; set; }
}