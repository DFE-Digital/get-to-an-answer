using System.Text.Json;
using Common.Client;
using Common.Domain;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Questions;

public class ReorderQuestions(ILogger<ReorderQuestions> logger, IApiClient apiClient)
    : BasePageModel
{
    [FromRoute] public Guid QuestionnaireId { get; set; }

    // The list we display on the page
    public List<QuestionDto> Questions { get; set; } = [];

    // Query params used by the move links
    [FromQuery] public Guid? QuestionId { get; set; }
    [FromQuery] public string? ActionType { get; set; } // "MoveUp" | "MoveDown"

    private string TempDataKey => $"QuestionOrder_{QuestionnaireId}";

    public async Task<IActionResult> OnGet()
    {
        BackLinkSlug = string.Format(Routes.AddAndEditQuestionsAndAnswers, QuestionnaireId);

        try
        {
            // 1. Load current order from TempData if it exists
            if (TempData.TryGetValue(TempDataKey, out var raw) && raw is string json && !string.IsNullOrWhiteSpace(json))
            {
                Questions = JsonSerializer.Deserialize<List<QuestionDto>>(json) ?? [];
            }
            else
            {
                // First time here: load from API and seed local state
                logger.LogInformation("Getting questions for questionnaire {QuestionnaireId}", QuestionnaireId);
                var response = await apiClient.GetQuestionsAsync(QuestionnaireId);

                Questions = response.OrderBy(q => q.Order).ToList();
            }

            // 2. Apply move action locally (no API calls)
            if (QuestionId.HasValue && !string.IsNullOrWhiteSpace(ActionType))
            {
                var index = Questions.FindIndex(q => q.Id == QuestionId.Value);
                if (index >= 0)
                {
                    if (string.Equals(ActionType, "MoveUp", StringComparison.OrdinalIgnoreCase) &&
                        index > 0)
                    {
                        (Questions[index - 1], Questions[index]) = (Questions[index], Questions[index - 1]);
                    }
                    else if (string.Equals(ActionType, "MoveDown", StringComparison.OrdinalIgnoreCase) &&
                             index < Questions.Count - 1)
                    {
                        (Questions[index], Questions[index + 1]) = (Questions[index + 1], Questions[index]);
                    }

                    Renumber();
                }
            }

            // 3. Save updated order back to TempData (local only)
            TempData[TempDataKey] = JsonSerializer.Serialize(Questions);
            TempData.Keep(TempDataKey);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error reordering questions for questionnaire {QuestionnaireId}", QuestionnaireId);
            return RedirectToErrorPage();
        }

        return Page();
    }

    private void Renumber()
    {
        for (var i = 0; i < Questions.Count; i++)
        {
            Questions[i].Order = i + 1;
        }
    }
}