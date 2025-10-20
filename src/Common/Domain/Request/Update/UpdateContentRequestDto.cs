using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateContentRequestDto
{
    public Guid Id { get; set; }
    public Guid QuestionnaireId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
} 
