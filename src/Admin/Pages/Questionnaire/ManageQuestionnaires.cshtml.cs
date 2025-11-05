using Admin.Models;
using Common.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Admin.Pages.Questionnaire;

public class ManageQuestionnaires(IApiClient apiClient) : QuestionnaireViewModel
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