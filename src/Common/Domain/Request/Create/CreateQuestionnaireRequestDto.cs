using System.ComponentModel.DataAnnotations;
using Common.Validation;

namespace Common.Domain.Request.Create;

public class CreateQuestionnaireRequestDto
{
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [MaxLength(500)]
    [GdsHeadContent(MinLength = 1, MaxLength = 500,
        ErrorMessage = "Title must be in plain language, avoid all-caps, not be empty or only whitespace, and should not contain repeated spaces.")]
    public required string Title { get; set; }
} 
