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
    public required QuestionnaireInfoDto Questionnaire { get; set; }
    public required DestinationDto Destination { get; set; }
    
    [FromRoute(Name = "questionnaireSlug")] 
    public new string? QuestionnaireSlug { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        HttpContext.Features.Set(new QuestionnaireRunFeature
        {
            IsEmbedded = Embed
        });
        
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

    public async Task<IActionResult> OnPost()
    {
        try
        {
            HttpContext.Features.Set(new QuestionnaireRunFeature
            {
                IsEmbedded = Embed
            });
        
            if (QuestionnaireSlug == null)
                return NotFound();
            
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);
        
            if (questionnaire == null)
                return NotFound();

            

            Questionnaire = questionnaire;
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetCurrentQuestion(Questionnaire.Id, NextStateRequest.CurrentQuestionId)
            };
            IsEmbedded = Embed;
            
            if (!ModelState.IsValid)
            {
                ModelState.Clear();
                
                // If we don't have a question OR we don't need to validate the answers, return the page
                if (Destination.Question is null || !NextStateRequest.ValidateAnswers) 
                    return Page();
                
                // Otherwise, set the error message based on the question type
                switch (Destination.Question.Type)
                {
                    case QuestionType.MultiSelect:
                        ModelState.AddModelError("NextStateRequest.SelectedAnswerIds",
                            "Select at least one answer");
                        break;
                    case QuestionType.SingleSelect:
                        ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select an answer");
                        break;
                    case QuestionType.DropdownSelect:
                        ModelState.AddModelError("NextStateRequest.SelectedAnswerIds", "Select an option");
                        break;
                }
                
                // Return the page with the errors added
                return Page();
            }
            
            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest);
            if (destination == null)
                return NotFound();
            
            if (!Embed && destination is { Type: DestinationType.ExternalLink, Content: not null })
            {
                var url = Uri.TryCreate(destination.Content, UriKind.Absolute, out var uri)
                    ? uri.ToString()
                    : "https://" + destination.Content;

                return Redirect(url);
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