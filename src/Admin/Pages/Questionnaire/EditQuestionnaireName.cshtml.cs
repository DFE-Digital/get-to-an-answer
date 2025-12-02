using System.ComponentModel.DataAnnotations;
using Common.Client;
using Common.Domain.Request.Update;
using Common.Enum;
using Common.Models;
using Common.Models.PageModels;
using Common.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class EditQuestionnaireName(IApiClient apiClient, ILogger<EditQuestionnaireName> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }
    
    [BindProperty(Name = "Title")]
    [Required(ErrorMessage = "Enter a questionnaire title")]
    [GdsTitle]
    public string? Title { get; set; }
    
    public IActionResult OnGet()
    {
        BackLinkSlug = string.Format(Routes.QuestionnaireTrackById, QuestionnaireId);
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            var updateQuestionnaireRequest = new UpdateQuestionnaireRequestDto { Title = Title };
            
            await apiClient.UpdateQuestionnaireAsync(QuestionnaireId, updateQuestionnaireRequest);
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });
            
            await apiClient.UpdateCompletionStateAsync(QuestionnaireId, new UpdateCompletionStateRequestDto
            {
                Task = CompletableTask.EditQuestionnaireName,
                Status = CompletionStatus.Completed // given the title is required
            });
            
            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToErrorPage();
        }
    }
}