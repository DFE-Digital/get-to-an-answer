using Common.Client;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Confirmations;

[Authorize]
public class ConfirmDeleteStartPage(IApiClient apiClient, 
    ILogger<ConfirmDeleteStartPage> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }
    
    [BindProperty] public bool RemoveStartPage { get; set; }

    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (RemoveStartPage)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                await apiClient.UpdateQuestionnaireAsync(QuestionnaireId, new UpdateQuestionnaireRequestDto
                {
                    DisplayTitle = string.Empty, // when empty the questionnaire run starts at the first question
                    Description = string.Empty,
                });
                
                await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
                {
                    Task = CompletableTask.AddStartPage,
                    Status = CompletionStatus.Optional
                });
            
                TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustRemovedStartPage = true });
            
                return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return RedirectToErrorPage();
            }
        }

        return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));
    }
}