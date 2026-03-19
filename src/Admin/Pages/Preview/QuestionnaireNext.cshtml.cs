using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Enum;
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
    public required QuestionnaireInfoDto Questionnaire { get; set; }
    public required DestinationDto Destination { get; set; }

    [FromQuery(Name = "embed")] 
    public bool Embed { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);

        if (questionnaire == null)
        {
            return NotFound();
        }

        Questionnaire = questionnaire;
        IsEmbedded = Embed;

        try
        {
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetInitialQuestion(questionnaire.Id, true)
            };
        }
        catch (GetToAnAnswerApiException)
        {
            ModelState.AddModelError(string.Empty, "Preview is not available until content has been added.");
            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            var questionnaire = await apiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
            
            if (questionnaire == null)
                return NotFound();
            
            Questionnaire = questionnaire;
            Destination = new DestinationDto
            {
                Type = DestinationType.Question,
                Question = await apiClient.GetCurrentQuestion(Questionnaire.Id, NextStateRequest.CurrentQuestionId, true)
            };
            IsEmbedded = Embed;
            
            if (!ModelState.IsValid)
            {
                ModelState.Clear();
                if (Destination.Question is not null && NextStateRequest.ValidateAnswers)
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
                return Page();
            }
        
            var destination = await apiClient.GetNextState(Questionnaire.Id, NextStateRequest, true);
            if (destination == null)
                return NotFound();

            // Redirect to external-link (if embedded or not, but different for Frontend, which is only standalone)
            if (destination is { Type: DestinationType.ExternalLink, Content: not null })
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