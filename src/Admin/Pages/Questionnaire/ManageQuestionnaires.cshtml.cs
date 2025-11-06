using Admin.Models;
using Admin.Models.PageModels;
using Common.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questionnaire;

public class ManageQuestionnaireses(IApiClient apiClient) : QuestionnairesPageModel
{
    public async Task<IActionResult> OnGet()
    {
        try
        {
            Questionnaires = await apiClient.GetQuestionnairesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return Page();
    }
}