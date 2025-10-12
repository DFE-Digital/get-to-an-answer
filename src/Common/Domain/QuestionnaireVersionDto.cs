using Common.Enum;

namespace Common.Domain;

public class QuestionnaireVersionDto
{
    public int Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int QuestionnaireId { get; set; }
    
    public int Version { get; set; }
    public string? QuestionnaireJson { get; set; }
}