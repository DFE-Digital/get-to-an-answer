using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class UpdateQuestionStatusRequestDto
{
    [Required(ErrorMessage = "Enter a question id")]   
    public Guid Id { get; set; }
    
    [EnumDefined]
    [Required(ErrorMessage = "Enter a question status")]
    public required EntityStatus Status { get; set; }
}