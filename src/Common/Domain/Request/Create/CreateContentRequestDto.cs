using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Create;

public class CreateContentRequestDto
{
    public Guid QuestionnaireId { get; set; }
    
    [Required(ErrorMessage = "Enter a content title")]
    [MaxLength(250, ErrorMessage = "Title must be 250 characters or fewer")]
    [GdsHeadContent(MinLength = 1, MaxLength = 500,
        ErrorMessage = "Title must be in plain language, avoid all-caps, not be empty or only whitespace, and should not contain repeated spaces.")]
    public required string Title { get; set; }
    
    [Required(ErrorMessage = "Enter some content")]
    [MaxLength(10000, ErrorMessage = "Content must be 10000 characters or fewer")]
    public required string Content { get; set; }
    public required string ReferenceName { get; set; }
} 
