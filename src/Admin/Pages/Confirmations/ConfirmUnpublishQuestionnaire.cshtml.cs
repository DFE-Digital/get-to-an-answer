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
    
    public async Task<IActionResult> OnPost([FromForm(Name = "UnpublishQuestionnaire")] bool unpublish = false)
    {
        if (unpublish)
        {
            var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
            
            if (questionnaire?.DecorativeImage is not null)
            {
                await imageStorageClient.DeleteImageAsync($"{QuestionnaireId}/published");
            }

            await apiClient.UnpublishQuestionnaireAsync(QuestionnaireId);
        }
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}