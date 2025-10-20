using Common.Domain;
using Common.Enum;

namespace Common.Domain;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid QuestionnaireId { get; set; }
    public int Order { get; set; }
    public string Content { get; set; }
    public string? Description { get; set; }
    public QuestionType Type { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
    public List<AnswerDto> Answers { get; set; } = new();
}