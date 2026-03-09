using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateCompletionStateRequestDto
{
    public required CompletableTask Task { get; set; }
    public required CompletionStatus Status { get; set; }
}