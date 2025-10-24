using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Create;

public class CloneQuestionnaireRequestDto
{
    public Guid OriginalQuestionnaireId { get; set; }
    
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [MaxLength(250, ErrorMessage = "Questionnaire title must be 250 characters or fewer")]
    public required string Title { get; set; }
} 
