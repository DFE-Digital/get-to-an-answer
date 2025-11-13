using Common.Client;
using Common.Domain.Request.Update;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class EditQuestionnaire(IApiClient apiClient, ILogger<EditQuestionnaire> logger) : QuestionnairesPageModel
{
    [FromRoute(Name = "questionnaireId")]
    public Guid QuestionnaireId { get; set; }
    
    [BindProperty]
    public required UpdateQuestionnaireRequestDto UpdateQuestionnaire { get; set; }

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
            
            await apiClient.UpdateQuestionnaireAsync(QuestionnaireId, UpdateQuestionnaire);
            
            TempData[nameof(QuestionnaireState)] = JsonConvert.SerializeObject(new QuestionnaireState { JustUpdated = true });
            
            return Redirect(string.Format(Routes.QuestionnaireTrackById, QuestionnaireId));

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToErrorPage();
        }
    }
}