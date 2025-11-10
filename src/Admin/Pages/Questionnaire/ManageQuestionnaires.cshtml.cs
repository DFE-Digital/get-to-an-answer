using Common.Models;
using Common.Models.PageModels;
using Common.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questionnaire;

[Authorize]
public class ManageQuestionnaires(IApiClient apiClient, ILogger<ManageQuestionnaires> logger) : QuestionnairesPageModel
{
    public async Task<IActionResult> OnGet()
    {
        try
        {
            ViewModel.Questionnaires = await apiClient.GetQuestionnairesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating questionnaire. Error: {EMessage}", e.Message);
            return RedirectToPage("/Error");
        }
        
        return Page();
    }
}