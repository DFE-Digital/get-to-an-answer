using Admin.Models;
using Admin.Models.PageModels;
using Common.Client;
using Common.Domain.Request.Update;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Admin.Pages.Questionnaire;

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
            
            return Redirect($"/admin/questionnaires/{QuestionnaireId}/edit");

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToPage("/Error");
        }
    }
}