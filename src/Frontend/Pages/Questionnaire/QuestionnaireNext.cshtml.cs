using Common.Domain;
using Common.Client;
using Common.Enum;
using Common.Models.PageModels;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Pages.Questionnaire;

[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnaireNavigationPageModel(apiClient, logger)
{
    [FromRoute(Name = "questionnaireSlug")] 
    public new string? QuestionnaireSlug { get; set; }

    public async Task<IActionResult> OnGet()
    {
        HttpContext.Features.Set(new QuestionnaireRunFeature
        {
            IsEmbedded = Embed
        });
        if (QuestionnaireSlug == null)
            return NotFound();
        var questionnaire = await ApiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);
        if (questionnaire == null)
            return NotFound();

        Questionnaire = questionnaire;
        IsEmbedded = Embed;
        Destination = new DestinationDto
        {
            Type = DestinationType.Question,
            Question = await ApiClient.GetInitialQuestion(questionnaire.Id)
        };

        return Page();
    }

    protected override async Task<QuestionnaireInfoDto?> GetQuestionnaireAsync()
    {
        if (QuestionnaireSlug == null)
            throw new InvalidOperationException("QuestionnaireSlug is required");

        return await ApiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireSlug);
    }

    protected override async Task<QuestionDto?> GetInitialQuestionAsync()
    {
        return await ApiClient.GetInitialQuestion(Questionnaire.Id);
    }

    protected override async Task<string> GetStartPageRouteAsync()
    {
        if (QuestionnaireSlug == null)
            throw new InvalidOperationException("QuestionnaireSlug is required");

        return await Task.FromResult($"/questionnaires/{QuestionnaireSlug}/start?embed={Embed}");
    }
}
