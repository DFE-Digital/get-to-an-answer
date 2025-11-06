using Common.Domain;

namespace Admin.Models.ViewModels;

public class QuestionnairesViewModel
{
    public List<QuestionnaireDto>? Questionnaires { get; set; }
    
    public QuestionnaireDto? Questionnaire { get; set; }
    
    public QuestionnaireState? QuestionnaireState { get; set; }
}