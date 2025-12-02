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
    
    [BindProperty] public bool PublishQuestionnaire { get; set; }
    
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

            try
            {
                await apiClient.PublishQuestionnaireAsync(QuestionnaireId);
            }
            catch (GetToAnAnswerApiException ex)
            {
                TempData["ApiAction"] = "Publishing the questionnaire failed.";
                TempData["ApiError"] = ex.ProblemDetails?.Detail;
                return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
            }

            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustPublished = true });
        }
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}