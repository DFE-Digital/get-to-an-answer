using System.Text.Json;
using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Preview;

[Authorize]
[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required GetNextStateRequest NextStateRequest { get; set; }
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public required QuestionnaireInfoDto Questionnaire { get; set; }
    [BindProperty] public required DestinationDto Destination { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
            
        if (questionnaire == null)
            return NotFound();

        Questionnaire = questionnaire;
        IsEmbedded = Embed;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await apiClient.GetInitialQuestion(questionnaire.Id, true)
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPost( 
        [FromForm(Name = "Priorities")] Dictionary<Guid, float> priorities)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ModelState.Clear();
                
                if (Destination.Question is not null)
                {
                    switch (Destination.Question.Type)
                    {
                        case QuestionType.MultiSelect:
                            ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select at least one answer");
                            break;
                        case QuestionType.SingleSelect:
                            ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select an answer");
                            break;
                        case QuestionType.DropdownSelect:
                            ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select an option");
                            break;
                    }
                }
                
                IsEmbedded = Embed;

                return Page();
            }
            
            Dictionary<Guid, float> finalPriorities = new();

            foreach (var (key, value) in priorities)
            {
                finalPriorities.Add(key, value <= 0 ? int.MaxValue : value);
            }

            if (NextStateRequest.SelectedAnswerIds.Count > 1)
            {
                var selectedAnswerId = finalPriorities
                    .Where(kv => NextStateRequest.SelectedAnswerIds.Contains(kv.Key))
                    .OrderBy(kv => kv.Value).First().Key;
                NextStateRequest.SelectedAnswerIds = [selectedAnswerId];
            } 
        
            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest, true);
        
            if (destination == null)
                return NotFound();

            // Redirect to external-link (if embedded or not, but different for Frontend, which is only standalone)
            if (destination is { Type: DestinationType.ExternalLink, Content: not null })
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