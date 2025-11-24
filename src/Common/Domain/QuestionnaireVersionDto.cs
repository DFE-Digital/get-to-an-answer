using Common.Enum;
using Common.Local;

namespace Common.Domain;

public class QuestionnaireVersionDto
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid QuestionnaireId { get; set; }
    
    public int Version { get; set; }
    public string? ChangeDescription { get; set; }
    public List<ChangeData>? ChangeLog { get; set; }
    public string? CreatedBy { get; set; }
}