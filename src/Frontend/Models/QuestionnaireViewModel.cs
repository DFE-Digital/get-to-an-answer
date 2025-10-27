using Common.Domain;
using Common.Domain.Frontend;

namespace Frontend.Models;

public class QuestionnaireViewModel : ConfigViewModel
{
    public QuestionnaireInfoDto? Questionnaire { get; set; }
    public GetNextStateRequest? NextStateRequest { get; set; }
    public DestinationDto? Destination { get; set; }
    public int? QuestionnaireId { get; set; }
    public int? QuestionId { get; set; }
    
    public bool AddEmptyAnswerOption { get; set; }
    public bool IsEmbedded { get; set; }
}