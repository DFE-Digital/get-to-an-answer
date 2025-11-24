using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Preview;

[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    [BindProperty] public required GetNextStateRequest NextStateRequest { get; set; }
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public bool IsRedirectConfirmation { get; set; } = false;
    [BindProperty] public required QuestionnaireInfoDto Questionnaire { get; set; }
    [BindProperty] public required DestinationDto Destination { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
        
        if (questionnaire == null)
            return NotFound();

        Questionnaire = new QuestionnaireInfoDto()
        {
            Id = questionnaire.Id,
            DisplayTitle = questionnaire.DisplayTitle ?? questionnaire.Title,
            Slug = questionnaire.Slug,
        };
        IsEmbedded = Embed;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await apiClient.GetInitialQuestion(questionnaire.Id, true)
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPost( 
        [FromForm(Name = "Scores")] Dictionary<Guid, float> scores, 
        [FromForm(Name = "ConfirmRedirect")] bool confirmRedirect)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (IsRedirectConfirmation)
            {
                if (confirmRedirect)
                {
                    var nextDestination = TempData["NextDestination"] as DestinationDto;
                    
                    if (nextDestination is null)
                    {
                        return NotFound();
                    }
                    
                    IsEmbedded = Embed;
                    Destination = nextDestination;
                    NextStateRequest = new GetNextStateRequest();
                    
                    if (Destination is { Type: DestinationType.ExternalLink, Content: not null })
                    {
                        return Redirect(Destination.Content);
                    }

                    return Page(); // display custom content page
                }
            }

            if (NextStateRequest.SelectedAnswerIds.Count > 1)
            {
                var selectedAnswerId = scores.OrderByDescending(kv => kv.Value).First().Key;
                NextStateRequest.SelectedAnswerIds = [selectedAnswerId];
            } 
        
            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest, true);
        
            if (destination == null)
                return NotFound();

            if (!Embed)
            {
                if (destination is { Type: DestinationType.ExternalLink, Content: not null } ||
                    destination is { Type: DestinationType.CustomContent, Title: not null, Content: not null })
                {
                    IsRedirectConfirmation = true;
                    
                    TempData["NextDestination"] = destination;
                    
                    return Page();
                }
            }

            IsEmbedded = Embed;
            Destination = destination;
            NextStateRequest = new GetNextStateRequest();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error load next question or final destination. Error: {EMessage}", e.Message);
            return RedirectToPage("/Error");
        }
        
        return Page();
    }
}