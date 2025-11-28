using Common.Models;
using Common.Models.PageModels;
using Common.Client;
using Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class TrackQuestionnaires(IApiClient apiClient, ILogger<TrackQuestionnaires> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")] 
    public new Guid? QuestionnaireId { get; set; }
    
    public QuestionnaireDto? Questionnaire { get; set; } = new();

    public async Task<IActionResult> OnGet()
    {
        try
        {
            if (QuestionnaireId == null)
                return Page();

            Questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId.Value);
            
            TempData["QuestionnaireStatus"] = (int) Questionnaire?.Status!;
            TempData.Keep("QuestionnaireStatus");
            
            TempData["QuestionnaireTitle"] = Questionnaire?.Title;
            TempData["QuestionnaireSlug"] = Questionnaire?.Slug;
            TempData["CompletionTrackingMap"] = JsonConvert.SerializeObject(Questionnaire?.CompletionTrackingMap);
            TempData.Keep("CompletionTrackingMap");
            
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
            return RedirectToPage("/error");
        }

        return Page(); 
    }
}