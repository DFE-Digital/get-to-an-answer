using Common.Domain;

namespace Admin.Models;

public class QuestionnaireViewModel : ConfigViewModel
{
    public List<QuestionnaireDto>? Questionnaires { get; set; }
    public QuestionnaireDto? Questionnaire { get; set; }
    public List<QuestionDto>? Questions { get; set; }
}