using Common.Enum;

namespace Common.Domain;

public class QuestionnaireDto
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public string? Slug { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
    
    public int Version { get; set; }

    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionnaireInfoDto
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public string? Slug { get; set; }
    
    public string? Description { get; set; }
}