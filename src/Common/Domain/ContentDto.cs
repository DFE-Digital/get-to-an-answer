using Common.Enum;

namespace Common.Domain;

public class ContentDto
{
    public Guid Id { get; set; }
    public Guid QuestionnaireId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}