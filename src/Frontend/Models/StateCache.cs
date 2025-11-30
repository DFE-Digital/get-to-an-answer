using Common.Domain;

namespace Frontend.Models;

public class StateCache
{
    public QuestionnaireInfoDto? Questionnaire { get; set; }
    public DestinationDto? NextDestination { get; set; }
    public DestinationDto? CurrDestination { get; set; }
}