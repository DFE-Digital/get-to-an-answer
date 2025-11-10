using Common.Models;
using Common.Domain;

namespace Common.Models.ViewModels;

public class QuestionnairesViewModel
{
    public List<QuestionnaireDto>? Questionnaires { get; set; }
    
    public QuestionnaireDto? Questionnaire { get; set; }
    
    public QuestionnaireState? QuestionnaireState { get; set; }
}