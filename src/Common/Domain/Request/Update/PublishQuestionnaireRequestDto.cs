using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class PublishQuestionnaireRequestDto
{
    [Required(ErrorMessage = "Enter a questionnaire id")]   
    public Guid Id { get; set; }
    
    [EnumDefined]
    [Required(ErrorMessage = "Enter a questionnaire status")]
    public required EntityStatus Status { get; set; }
}