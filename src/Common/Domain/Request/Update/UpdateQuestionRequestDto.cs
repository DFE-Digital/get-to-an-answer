using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class UpdateQuestionRequestDto
{
    [Required(ErrorMessage = "Enter question content")]
    [GdsTitle(MinLength = 1, MaxLength = 500,
        ErrorMessage = "Content must be in plain language, avoid all-caps, not be empty or only whitespace, and should not contain repeated spaces.")]
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    [EnumDefined]
    public QuestionType? Type { get; set; }
}