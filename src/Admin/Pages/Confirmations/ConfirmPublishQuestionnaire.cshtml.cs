using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmPublishQuestionnaire(IApiClient apiClient, IImageStorageClient imageStorageClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }

    public string? QuestionnaireTitle { get; set; }
    
    [BindProperty] public bool PublishQuestionnaire { get; set; }

    public void OnGet()
    {
        if (TempData.Peek("QuestionnaireTitle") is string title)
        {
            QuestionnaireTitle = title;
        }
    }
    
    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (PublishQuestionnaire)
        {
            var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
            
            if (questionnaire?.DecorativeImage is not null) 
            {
                await imageStorageClient.DuplicateToAsync(
                    $"{QuestionnaireId}/latest", $"{QuestionnaireId}/published");
            }

            await apiClient.PublishQuestionnaireAsync(QuestionnaireId);
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustPublished = true });
        }
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}