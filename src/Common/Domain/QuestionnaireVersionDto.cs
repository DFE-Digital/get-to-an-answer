using Common.Enum;

namespace Common.Domain;

public class QuestionnaireVersionDto
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid QuestionnaireId { get; set; }
    
    public int Version { get; set; }
    public string? QuestionnaireJson { get; set; }
}