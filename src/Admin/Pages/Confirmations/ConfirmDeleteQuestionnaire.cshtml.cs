using Common.Client;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmDeleteQuestionnaire(IApiClient apiClient) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }

    public string? QuestionnaireTitle { get; set; }
    
    [BindProperty] public bool DeleteQuestionnaire { get; set; }

    public void OnGet()
    {
        if (TempData.Peek("QuestionnaireTitle") is string title)
        {
            QuestionnaireTitle = title;
        }
    }

    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (DeleteQuestionnaire)
        {
            await apiClient.DeleteQuestionnaireAsync(QuestionnaireId);
            
            TempData[nameof(QuestionnaireState)] = new QuestionnaireState { JustDeleted = true };
        }
        
        return Redirect(Routes.QuestionnairesManage);
    }
}