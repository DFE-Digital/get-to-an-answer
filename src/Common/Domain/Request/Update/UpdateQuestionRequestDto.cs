using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionRequestDto
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public string? Description { get; set; }
    
    public required QuestionType Type { get; set; }
} 
