using Common.Client;
using Common.Domain;
using Common.Enum;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Preview;

[Authorize]
[IgnoreAntiforgeryToken]
public class QuestionnaireNext(IApiClient apiClient, ILogger<QuestionnaireNext> logger) : QuestionnaireNavigationPageModel(apiClient, logger)
{
    [FromRoute(Name = "questionnaireId")] 
    public required Guid QuestionnaireId { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var questionnaire = await ApiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);

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
                Question = await ApiClient.GetInitialQuestion(questionnaire.Id, true)
            };
        }
        catch (GetToAnAnswerApiException)
        {
            ModelState.AddModelError(string.Empty, "Preview is not available until content has been added.");
            return Page();
        }

        return Page();
    }

    protected override async Task<IActionResult> HandleContinueAsync()
    {
        try
        {
            return await base.HandleContinueAsync();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    protected override async Task<IActionResult> HandleBackAsync()
    {
        try
        {
            return await base.HandleBackAsync();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    protected override async Task<QuestionnaireInfoDto?> GetQuestionnaireAsync()
    {
        return await ApiClient.GetLastPublishedQuestionnaireInfoAsync(QuestionnaireId.ToString(), true);
    }

    protected override async Task<QuestionDto?> GetInitialQuestionAsync()
    {
        return await ApiClient.GetInitialQuestion(Questionnaire.Id, true);
    }

    protected override async Task<string> GetStartPageRouteAsync()
    {
        return await Task.FromResult($"/admin/questionnaires/{QuestionnaireId}/start-preview?embed={Embed}");
    }

    protected override bool GetPreviewFlag() => true;
}
