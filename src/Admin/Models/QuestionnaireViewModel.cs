using Common.Domain;
using Common.Domain.Frontend;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;

namespace Admin.Models;

public class QuestionnaireViewModel : ConfigViewModel
{
    public List<QuestionnaireDto>? Questionnaires { get; set; }
    public QuestionnaireDto? Questionnaire { get; set; }
    public ContentDto? Content { get; set; }
    public List<ContentDto>? Contents { get; set; }
    
    public CreateContentRequestDto? CreateContent { get; set; }
    
    public UpdateContentRequestDto? UpdateContent { get; set; }
    
    public CreateQuestionnaireRequestDto? CreateQuestionnaire { get; set; }
    
    public UpdateQuestionnaireRequestDto? UpdateQuestionnaire { get; set; }
    
    public CloneQuestionnaireRequestDto? CloneQuestionnaire { get; set; }
    
    public CreateQuestionRequestDto? CreateQuestion { get; set; }
    
    public UpdateQuestionRequestDto? UpdateQuestion { get; set; }
    
    public CreateAnswerRequestDto? CreateAnswer { get; set; }
    
    public CreateAnswerRequestDto? UpdateAnswer { get; set; }
    public List<QuestionDto>? Questions { get; set; }
    public QuestionDto? Question { get; set; }
    public List<AnswerDto>? Answers { get; set; }
    
    public GetNextStateRequest? NextStateRequest { get; set; }
    public DestinationDto? Destination { get; set; }
    public Guid? QuestionnaireId { get; set; }
    public Guid? QuestionId { get; set; }
    
    public bool AddEmptyAnswerOption { get; set; }
    
    public bool InviteAccepted { get; set; }
    
    public string OtherVersionHtml { get; init; } = "";
    public int OtherVersion { get; init; }
    public string CurrentVersionHtml { get; init; } = "";
    public int CurrentVersion { get; init; }
    public List<QuestionnaireVersionDto> QuestionnaireVersions { get; set; } = new();
    public bool JustCreated { get; set; }
    public bool JustCloned { get; set; }
    public bool JustUpdated { get; set; }
    public bool JustDeleted { get; set; }
    public bool JustMovedUp { get; set; }
    public bool JustMovedDown { get; set; }
    public bool JustPublished { get; set; }
    public bool JustUnpublished { get; set; }
    public string[] Contributors { get; set; } = [];
}