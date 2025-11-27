using Common.Enum;

namespace Common.Versioning;

public class QuestionnaireContent
{
    public string? Title { get; set; }
    public string? DisplayTitle { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public EntityStatus? Status { get; set; }
    
    public Dictionary<string, bool>? CompletionTrackingMap { get; set; }
    
    public string? TextColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? PrimaryButtonColor { get; set; }
    public string? SecondaryButtonColor { get; set; }
    public string? StateColor { get; set; }
    public string? ErrorMessageColor { get; set; }
    
    public string? ContinueButtonText { get; set; }
    
    public string? DecorativeImage { get; set; }
    
    public bool? IsAccessibilityAgreementAccepted { get; set; }
    
    public List<QuestionContent>? Questions { get; set; }
    
    public List<ContentContent>? Contents { get; set; }
}

public class QuestionContent
{
    public string? Content { get; set; }
    public string? Description { get; set; }
    public int? Order { get; set; }
    public EntityStatus Status { get; set; }
    public QuestionType Type { get; set; }
    
    public List<AnswerContent>? Answers { get; set; }
}

public class AnswerContent
{
    public string? Content { get; set; }
    public string? Description { get; set; }
    public float Priority { get; set; }
    public DestinationType? DestinationType { get; set; }
    
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
    
    public Guid? DestinationContentId { get; set; }
}

public class ContentContent
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ReferenceName { get; set; }
}