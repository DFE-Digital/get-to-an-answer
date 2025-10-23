using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class UpdateQuestionnaireRequestDto
{
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [MaxLength(500)]
    [GdsTitle(MinLength = 1, MaxLength = 500,
        ErrorMessage = "Title must be in plain language, avoid all-caps, not be empty or only whitespace, and should not contain repeated spaces.")]
    public required string Title { get; set; }
    
    [MaxLength(500)]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", 
        ErrorMessage = "Slug must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen")]
    public string? Slug { get; set; }
    
    [MaxLength(10000, ErrorMessage = "Description must be 10,000 characters or fewer")]
    public string? Description { get; set; }
}