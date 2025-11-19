using Common.Client;
using Common.Domain;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questions;

public class AddEditQuestionsAndAnswers(ILogger<AddEditQuestionsAndAnswers> logger, IApiClient apiClient)
    : BasePageModel
{
    [FromRoute] public Guid QuestionnaireId { get; set; }

    [BindProperty] public List<QuestionDto> Questions { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddQuestion, QuestionnaireId);

        try
        {
            logger.LogInformation("Getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            var response = await apiClient.GetQuestionsAsync(QuestionnaireId);

            Questions = response.OrderBy(q => q.Order).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostMoveUpAsync(Guid questionnaireId, Guid questionId)
    {
        return await HandleRequest(questionnaireId, questionId, handler: "MoveUp");
    }


    public async Task<IActionResult> OnPostMoveDownAsync(Guid questionnaireId, Guid questionId)
    {
        return await HandleRequest(questionnaireId, questionId, handler: "MoveDown");
    }

    private async Task<IActionResult> HandleRequest(Guid questionnaireId, Guid questionId, string? handler)
    {
        try
        {
            await MoveQuestion(handler, questionnaireId, questionId);

            Questions = (await apiClient.GetQuestionsAsync(questionnaireId)).OrderBy(q => q.Order).ToList();
            QuestionnaireId = questionnaireId;

            return Page();
        }
        catch (HttpRequestException requestException) when (requestException.StatusCode ==
                                                            System.Net.HttpStatusCode.BadRequest)
        {
            var direction = handler == "MoveUp" ? "up" : "down";
            ModelState.AddModelError("QuestionMoveError", $"You cannot move this question any further {direction}.");

            Questions = (await apiClient.GetQuestionsAsync(questionnaireId)).OrderBy(q => q.Order).ToList();
            QuestionnaireId = questionnaireId;

            return Page();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error moving question up for questionnaire {QuestionnaireId}", questionnaireId);
            return RedirectToErrorPage();
        }
    }

    private async Task MoveQuestion(string? handler, Guid questionnaireId, Guid questionId)
    {
        switch (handler, !string.IsNullOrWhiteSpace(handler))
        {
            case ("MoveUp", _):
                await apiClient.MoveQuestionUpOneAsync(questionnaireId, questionId);
                break;
            case ("MoveDown", _):
                await apiClient.MoveQuestionDownOneAsync(questionnaireId, questionId);
                break;
        }
    }
}