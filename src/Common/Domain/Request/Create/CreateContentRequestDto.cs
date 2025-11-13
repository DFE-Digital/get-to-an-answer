using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Create;

public class CreateContentRequestDto
{
    public Guid QuestionnaireId { get; set; }
    
    [Required(ErrorMessage = "Enter a content title")]
    [MaxLength(250, ErrorMessage = "Title must be 250 characters or fewer")]
    public required string Title { get; set; }
    
    [Required(ErrorMessage = "Enter some content")]
    [MaxLength(10000, ErrorMessage = "Content must be 10000 characters or fewer")]
    public required string Content { get; set; }
    public required string ReferenceName { get; set; }
} 
