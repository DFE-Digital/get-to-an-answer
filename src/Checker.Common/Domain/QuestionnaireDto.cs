using Checker.Common.Enum;

namespace Checker.Common.Domain;

public class QuestionnaireDto
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
}