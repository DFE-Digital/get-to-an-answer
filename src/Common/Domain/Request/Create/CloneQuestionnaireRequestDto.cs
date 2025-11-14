using System.ComponentModel.DataAnnotations;
using Common.Validation;

namespace Common.Domain.Request.Create;

public class CloneQuestionnaireRequestDto
{
    public Guid OriginalQuestionnaireId { get; set; }
    
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [MaxLength(250, ErrorMessage = "Questionnaire title must be 250 characters or fewer")]
    [GdsTitle(MinLength = 1, MaxLength = 500,
        ErrorMessage = "Title must be in plain language, avoid all-caps, not be empty or only whitespace, and should not contain repeated spaces.")]
    public required string Title { get; set; }
} 
