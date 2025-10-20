using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionnaireStatusRequestDto
{
    public Guid Id { get; set; }
    public required EntityStatus Status { get; set; }
}