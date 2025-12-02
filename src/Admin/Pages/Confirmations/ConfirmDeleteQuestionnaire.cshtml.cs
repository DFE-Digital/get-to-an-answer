using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmDeleteQuestionnaire(IApiClient apiClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }
    
    [BindProperty] public bool DeleteQuestionnaire { get; set; }

    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (DeleteQuestionnaire)
        {
            await apiClient.DeleteQuestionnaireAsync(QuestionnaireId);
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustDeleted = true });
        
            return Redirect(Routes.QuestionnairesManage);
        }
        
        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}