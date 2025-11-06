using Admin.Models;
using Admin.Models.ViewModels;
using Common.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

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

            QuestionnairesViewModel = TempData.TryGetValue(nameof(QuestionnairesViewModel), out var value)
                ? JsonConvert.DeserializeObject<QuestionnairesViewModel>(value?.ToString() ?? string.Empty) ??
                  new QuestionnairesViewModel()
                : new QuestionnairesViewModel();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Page();
    }
}