using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmPublishQuestionnaire(IApiClient apiClient, IImageStorageClient imageStorageClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }
    
    public async Task<IActionResult> OnPost([FromForm(Name = "PublishQuestionnaire")] bool publish = false)
    {
        if (publish)
        {
            var questionnaire = await apiClient.GetQuestionnaireAsync(QuestionnaireId);
            
            if (questionnaire?.DecorativeImage is not null) 
            {
                await imageStorageClient.DuplicateToAsync(
                    $"{QuestionnaireId}/latest", $"{QuestionnaireId}/published");
            }

            await apiClient.PublishQuestionnaireAsync(QuestionnaireId);
        }
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}