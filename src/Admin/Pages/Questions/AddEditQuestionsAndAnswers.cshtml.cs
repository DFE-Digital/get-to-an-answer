using Common.Client;
using Common.Domain;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;

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

            // If a move error message was set during a previous POST, surface it in ModelState
            if (TempData.TryGetValue("MoveError", out var moveErrorObj) && moveErrorObj is string moveError &&
                !string.IsNullOrWhiteSpace(moveError))
            {
                ModelState.AddModelError("QuestionMoveError", moveError);
            }
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
        try
        {
            await apiClient.MoveQuestionUpOneAsync(questionnaireId, questionId);

            // PRG - redirect back to GET for the same questionnaire so refresh won't resubmit
            return RedirectToPage(new { questionnaireId });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Set an error message in TempData and redirect back (PRG)
            TempData["MoveError"] = "You cannot move this question further up";
            return RedirectToPage(new { questionnaireId });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error moving question up for questionnaire {QuestionnaireId}", questionnaireId);
            return RedirectToErrorPage();
        }
    }

    public async Task<IActionResult> OnPostMoveDownAsync(Guid questionnaireId, Guid questionId)
    {
        try
        {
            await apiClient.MoveQuestionDownOneAsync(questionnaireId, questionId);

            // PRG - redirect back to GET for the same questionnaire so refresh won't resubmit
            return RedirectToPage(new { questionnaireId });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Set an error message in TempData and redirect back (PRG)
            TempData["MoveError"] = "You cannot move this question further down.";
            return RedirectToPage(new { questionnaireId });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error moving question down for questionnaire {QuestionnaireId}", questionnaireId);
            return RedirectToErrorPage();
        }
    }
}