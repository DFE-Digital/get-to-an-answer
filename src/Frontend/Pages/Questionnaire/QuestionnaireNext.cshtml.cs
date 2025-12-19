using System.Buffers.Text;
using System.Text.Json;
using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages.Questionnaire;

[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnairesPageModel
{
    [BindProperty] public required GetNextStateRequest NextStateRequest { get; set; }
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public required QuestionnaireInfoDto Questionnaire { get; set; }
    [BindProperty] public required DestinationDto Destination { get; set; }
    
    [FromRoute(Name = "questionnaireSlug")] 
    public new string? QuestionnaireSlug { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }
    
    [BindProperty] public string? StateCacheString { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (QuestionnaireSlug == null)
            return NotFound();
        
        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);
        
        if (questionnaire == null)
            return NotFound();

        Questionnaire = questionnaire;
        IsEmbedded = Embed;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await apiClient.GetInitialQuestion(questionnaire.Id)
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPost( 
        [FromForm(Name = "Priorities")] Dictionary<Guid, float> priorities)
    {
        try
        {
            if (QuestionnaireSlug == null)
                return NotFound();
            
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            Dictionary<Guid, float> finalPriorities = new();

            foreach (var (key, value) in priorities)
            {
                finalPriorities.Add(key, value == 0 ? int.MaxValue : value);
            }

            if (NextStateRequest.SelectedAnswerIds.Count > 1)
            {
                var selectedAnswerId = finalPriorities
                    .Where(kv => NextStateRequest.SelectedAnswerIds.Contains(kv.Key))
                    .OrderBy(kv => kv.Value).First().Key;
                NextStateRequest.SelectedAnswerIds = [selectedAnswerId];
            }
        
            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest);
        
            if (destination == null)
                return NotFound();

            if (!Embed && destination is { Type: DestinationType.ExternalLink, Content: not null })
            {
                return Redirect(destination.Content);
            }

            IsEmbedded = Embed;
            Destination = destination;
            NextStateRequest = new GetNextStateRequest();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error load next question or final destination. Error: {EMessage}", e.Message);
            throw;
        }
        
        return Page();
    }
}