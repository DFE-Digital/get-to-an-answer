using Admin.Models;
using Admin.Models.ViewModels;
using Common.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Questionnaire;

public class TackQuestionnaire(IApiClient apiClient) : QuestionnairePageModel
{
    [FromRoute(Name = "questionnaireId")] public new Guid? QuestionnaireId { get; set; }

    public QuestionnairesViewModel QuestionnairesViewModel { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        try
        {
            if (QuestionnaireId == null)
                return Page();

            Questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId.Value);

            QuestionnairesViewModel = TempData["QuestionnairesViewModel"] as QuestionnairesViewModel ??
                                      new QuestionnairesViewModel();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Page();
    }
}