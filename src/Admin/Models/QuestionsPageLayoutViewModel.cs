namespace Admin.Models;

public class QuestionsPageLayoutViewModel
{
    public Guid QuestionnaireId { get; set; }

    public string Heading { get; set; } = string.Empty;

    public bool ShowDraftTag { get; set; }

    public string? IntroText { get; set; }

    public QuestionsListViewModel QuestionsList { get; set; } = new();
}