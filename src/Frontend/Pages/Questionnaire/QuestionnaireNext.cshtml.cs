using System.Text.Json;
using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages.Questionnaire;

[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnairesPageModel
{
    [BindProperty] public required GetNextStateRequest NextStateRequest { get; set; }
    [BindProperty] public required bool IsEmbedded { get; set; }
    [BindProperty] public bool IsRedirectConfirmation { get; set; } = false;
    [BindProperty] public required QuestionnaireInfoDto Questionnaire { get; set; }
    [BindProperty] public required DestinationDto Destination { get; set; }
    
    [FromRoute(Name = "questionnaireSlug")] 
    public new string? QuestionnaireSlug { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

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
        [FromForm(Name = "Scores")] Dictionary<Guid, float> scores, 
        [FromForm(Name = "ConfirmRedirect")] bool confirmRedirect)
    {
        try
        {
            if (QuestionnaireSlug == null)
                return NotFound();

            if (IsRedirectConfirmation)
            {
                IsRedirectConfirmation = false;
                
                if (confirmRedirect)
                {
                    var nextDestination = JsonSerializer.Deserialize<DestinationDto>(TempData["NextDestination"]?.ToString() ?? "{}");
                    
                    if (nextDestination is null)
                    {
                        return NotFound();
                    }
                    
                    IsEmbedded = Embed;
                    Destination = nextDestination;
                    NextStateRequest = new GetNextStateRequest();
                    Questionnaire = JsonSerializer.Deserialize<QuestionnaireInfoDto>(TempData["Questionnaire"]?.ToString() ?? "{}")!;

                    if (!Embed)
                    {
                        if (Destination is { Type: DestinationType.ExternalLink, Content: not null })
                        {
                            return Redirect(Destination.Content);
                        }
                    }
                }
                else
                {
                    ModelState.Clear();
                    
                    Questionnaire = JsonSerializer.Deserialize<QuestionnaireInfoDto>(TempData["Questionnaire"]?.ToString() ?? "{}")!;
                    Destination = JsonSerializer.Deserialize<DestinationDto>(TempData["CurrDestination"]?.ToString() ?? "{}")!;
                }

                return Page();
            }
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (NextStateRequest.SelectedAnswerIds.Count > 1)
            {
                var selectedAnswerId = scores.OrderByDescending(kv => kv.Value).First().Key;
                NextStateRequest.SelectedAnswerIds = [selectedAnswerId];
            } 
        
            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest);
        
            if (destination == null)
                return NotFound();

            if (destination is { Type: DestinationType.ExternalLink, Content: not null } ||
                destination is { Type: DestinationType.CustomContent, Title: not null, Content: not null })
            {
                IsRedirectConfirmation = true;
                    
                TempData["Questionnaire"] = JsonSerializer.Serialize(Questionnaire);
                TempData["CurrDestination"] = JsonSerializer.Serialize(Destination);
                TempData["NextDestination"] = JsonSerializer.Serialize(destination);
                    
                return Page();
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