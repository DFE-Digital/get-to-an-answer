using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionRequestDto
{
    public required string Content { get; set; }
    public required string Description { get; set; }
} 
