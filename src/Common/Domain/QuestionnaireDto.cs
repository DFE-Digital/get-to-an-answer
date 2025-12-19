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
    
    // ~~~ Look and feel ~~~
    
    public string? TextColor { get; set; } = "#0b0c0c";
    public string? BackgroundColor { get; set; } = "#ffffff";
    public string? PrimaryButtonColor { get; set; } = "#00703c";
    public string? SecondaryButtonColor { get; set; } = "#1d70b8";
    public string? StateColor { get; set; } = "#ffdd00";
    public string? ErrorMessageColor { get; set; } = "#c3432b";
    
    public string? DecorativeImage { get; set; }

    public bool IsAccessibilityAgreementAccepted { get; set; } = false;
    public string? ContinueButtonText { get; set; }
    public Dictionary<CompletableTask, CompletionStatus>? CompletionTrackingMap { get; set; }
    public bool IsUnpublished { get; set; }
}

public class QuestionnaireInfoDto
{
    public Guid Id { get; set; }
    
    public string? DisplayTitle { get; set; }
    
    public string? Slug { get; set; }
    
    public string? Description { get; set; }
    
    public bool HasStartPage { get; set; }
    
    // ~~~ Look and feel ~~~
    
    public string? TextColor { get; set; } = "#0b0c0c";
    public string? BackgroundColor { get; set; } = "#ffffff";
    public string? PrimaryButtonColor { get; set; } = "#00703c";
    public string? SecondaryButtonColor { get; set; } = "#1d70b8";
    public string? StateColor { get; set; } = "#ffdd00";
    public string? ErrorMessageColor { get; set; } = "#c3432b";
    
    public string? DecorativeImage { get; set; }
    public string? ContinueButtonText { get; set; } = "Continue";
}