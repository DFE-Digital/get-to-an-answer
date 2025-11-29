using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmUnpublishQuestionnaire(IApiClient apiClient, IImageStorageClient imageStorageClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }

    public string? QuestionnaireTitle { get; set; }
    
    [BindProperty] public bool UnpublishQuestionnaire { get; set; }

    public void OnGet()
    {
        if (TempData.Peek("QuestionnaireTitle") is string title)
        {
            QuestionnaireTitle = title;
        }
    }
    
    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (UnpublishQuestionnaire)
        {
            var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
            
            if (questionnaire?.DecorativeImage is not null)
            {
                await imageStorageClient.DeleteImageAsync($"{QuestionnaireId}/published");
            }

            await apiClient.UnpublishQuestionnaireAsync(QuestionnaireId);
            
            TempData[nameof(QuestionnaireState)] = new QuestionnaireState { JustUnpublished = true };
        }
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}