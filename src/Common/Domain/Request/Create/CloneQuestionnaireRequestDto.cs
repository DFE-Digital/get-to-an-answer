using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Request.Create;

public class CloneQuestionnaireRequestDto
{
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [MaxLength(250, ErrorMessage = "Questionnaire title must be 250 characters or fewer")]
    public required string Title { get; set; }
    
    [MaxLength(1000, ErrorMessage = "Description must be 500 characters or fewer")]
    public string? Description { get; set; }
}