using Common.Domain;
using Common.Domain.Frontend;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;

namespace Admin.Models;

public class QuestionnaireViewModel : ConfigViewModel
{
    public List<QuestionnaireDto>? Questionnaires { get; set; }
    public QuestionnaireDto? Questionnaire { get; set; }
    
    public CreateQuestionnaireRequestDto? CreateQuestionnaire { get; set; }
    
    public UpdateQuestionnaireRequestDto? UpdateQuestionnaire { get; set; }
    
    public CreateQuestionRequestDto? CreateQuestion { get; set; }
    
    public UpdateQuestionRequestDto? UpdateQuestion { get; set; }
    
    public CreateAnswerRequestDto? CreateAnswer { get; set; }
    
    public CreateAnswerRequestDto? UpdateAnswer { get; set; }
    public List<QuestionDto>? Questions { get; set; }
    public QuestionDto? Question { get; set; }
    public List<AnswerDto>? Answers { get; set; }
    
    public GetNextStateRequest? NextStateRequest { get; set; }
    public DestinationDto? Destination { get; set; }
    public int? QuestionnaireId { get; set; }
    public int? QuestionId { get; set; }
    
    public bool AddEmptyAnswerOption { get; set; }
}