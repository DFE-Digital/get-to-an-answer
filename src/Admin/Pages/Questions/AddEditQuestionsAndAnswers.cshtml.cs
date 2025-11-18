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
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }

        return Page();
    }
    
    // Move a question down in the list (no API call yet)
    public IActionResult OnPostMoveDown(int index)
    {
        if (!ModelState.IsValid || Questions is null)
        {
            return Page();
        }

        if (index < 0 || index >= Questions.Count - 1)
        {
            return Page();
        }

        (Questions[index], Questions[index + 1]) = (Questions[index + 1], Questions[index]);

        RenumberQuestions();

        return Page();
    }

    // This would be called by your "Save and continue" form to persist ordering
    public async Task<IActionResult> OnPostSave()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // TODO: call an API endpoint to persist new order, e.g.
            // await apiClient.UpdateQuestionOrderAsync(QuestionnaireId, Questions);

            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error saving question order for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }
    }

    private void RenumberQuestions()
    {
        for (var i = 0; i < Questions.Count; i++)
        {
            Questions[i].Order = i + 1;
        }
    }
}