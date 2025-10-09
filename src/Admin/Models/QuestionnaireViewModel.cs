using Common.Domain;
using Common.Domain.Frontend;
using Common.Domain.Request.Create;

namespace Admin.Models;

public class QuestionnaireViewModel : ConfigViewModel
{
    public List<QuestionnaireDto>? Questionnaires { get; set; }
    
    public CreateQuestionnaireRequestDto? CreateQuestionnaire { get; set; }
    public QuestionnaireDto? Questionnaire { get; set; }
    public List<QuestionDto>? Questions { get; set; }
    
    public GetNextStateRequest? NextStateRequest { get; set; }
    public DestinationDto? Destination { get; set; }
}