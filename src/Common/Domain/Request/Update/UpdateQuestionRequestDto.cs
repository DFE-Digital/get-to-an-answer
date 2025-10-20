using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionRequestDto
{
    public Guid Id { get; set; }
    public Guid QuestionnaireId { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public required QuestionType Type { get; set; }
} 
