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
}