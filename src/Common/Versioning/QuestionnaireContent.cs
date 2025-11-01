using Common.Enum;

namespace Common.Versioning;

public class QuestionnaireContent
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public EntityStatus? Status { get; set; }
    
    public List<QuestionContent>? Questions { get; set; }
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
    public DestinationType? DestinationType { get; set; }
    
    public string? DestinationUrl { get; set; }
    
    public int? DestinationQuestionId { get; set; }
    
    public int? DestinationContentId { get; set; }
}