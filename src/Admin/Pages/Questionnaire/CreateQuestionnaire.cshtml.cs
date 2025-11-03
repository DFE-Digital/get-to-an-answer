using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Questionnaire;

public class CreateQuestionnaire : QuestionnaireViewModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
    
    public void OnPost()
    {
        
    }
}