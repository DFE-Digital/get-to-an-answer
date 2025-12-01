using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;
using Common.Validation;

namespace Common.Domain.Request.Update;

public class UpdateCompletionStateRequestDto
{
    public required CompletableTask Task { get; set; }
    public required CompletionStatus Status { get; set; }
}