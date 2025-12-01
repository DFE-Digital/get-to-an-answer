using System.Text.Json;
using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
using Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Preview;

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
        var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
        
        if (questionnaire == null)
            return NotFound();

        Questionnaire = new QuestionnaireInfoDto()
        {
            Id = questionnaire.Id,
            DisplayTitle = questionnaire.DisplayTitle ?? questionnaire.Title,
            Slug = questionnaire.Slug,
            
            DecorativeImage = questionnaire.DecorativeImage,
            
            TextColor = questionnaire.TextColor,
            BackgroundColor = questionnaire.BackgroundColor,
            PrimaryButtonColor = questionnaire.PrimaryButtonColor,
            SecondaryButtonColor = questionnaire.SecondaryButtonColor,
            StateColor = questionnaire.StateColor,
            ErrorMessageColor = questionnaire.ErrorMessageColor,
            
            ContinueButtonText = questionnaire.ContinueButtonText
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
        [FromForm(Name = "Priorities")] Dictionary<Guid, float> priorities)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (NextStateRequest.SelectedAnswerIds.Count > 1)
            {
                var selectedAnswerId = priorities.OrderBy(kv => kv.Value).First().Key;
                NextStateRequest.SelectedAnswerIds = [selectedAnswerId];
            } 
        
            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest, true);
        
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