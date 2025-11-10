using Common.Models;
using Common.Models.PageModels;
using Common.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

public class TrackQuestionnaires(IApiClient apiClient, ILogger<TrackQuestionnaires> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public new Guid? QuestionnaireId { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            if (QuestionnaireId == null)
                return Page();

            ViewModel.Questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId.Value);
            
            var stateFound = TempData.TryGetValue(nameof(QuestionnaireState), out var value);

            if (stateFound)
            {
                var deserialized = JsonConvert.DeserializeObject<QuestionnaireState>(value?.ToString() ?? string.Empty);
                if (deserialized == null)
                    logger.LogError("Failed to deserialize QuestionnaireState from TempData.");
                
                ViewModel.QuestionnaireState = deserialized;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToPage("/Error");
        }

        return Page(); 
    }
}