using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Request.Update;

public class UpdateContinueButtonRequestDto
{
    [MaxLength(50)] public string? ContinueButtonText { get; set; } = "Continue";
}