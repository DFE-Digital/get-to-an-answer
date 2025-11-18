namespace Admin.Models;

public class QuestionsListViewModel
{
    public List<QuestionViewModel> Questions { get; set; } = [];
}

public class QuestionViewModel
{
    public Guid QuestionId { get; set; }

    public Guid QuestionnaireId { get; set; }
    public int Order { get; set; }
    
    public string Content { get; set; } = string.Empty;
}