using Common.Enum;

namespace Common.Domain;

public class ContentDto
{
    public int Id { get; set; }
    public int QuestionnaireId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}