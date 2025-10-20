using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionStatusRequestDto
{
    public Guid Id { get; set; }
    public required EntityStatus Status { get; set; }
} 
