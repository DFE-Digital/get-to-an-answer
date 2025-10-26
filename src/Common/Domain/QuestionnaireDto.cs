using Common.Enum;

namespace Common.Domain;

public class QuestionnaireDto
{
    public Guid Id { get; set; }
    
    public string? DisplayTitle { get; set; }
    
    public string Title { get; set; }
    
    public string? Slug { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
    
    public int Version { get; set; }

    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionnaireInfoDto
{
    public Guid Id { get; set; }
    
    public string? DisplayTitle { get; set; }
    
    public string? Slug { get; set; }
    
    public string? Description { get; set; }
}