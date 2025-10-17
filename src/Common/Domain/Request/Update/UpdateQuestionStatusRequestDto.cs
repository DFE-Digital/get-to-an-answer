using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionnaireStatusRequestDto
{
    [Required(ErrorMessage = "Enter a questionnaire id")]   
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Enter a questionnaire status")]
    public required EntityStatus Status { get; set; }
}