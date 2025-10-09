using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionnaireStatusRequestDto
{
    public int Id { get; set; }
    public required EntityStatus Status { get; set; }
}