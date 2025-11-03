using Admin.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Views.Questionnaire;

public class CreateQuestionnaire : PageModel
{
    public QuestionnaireViewModel QuestionnaireViewModel { get; set; } = new();
    
    
    
    public void OnGet()
    {
        
    }
    
    public void OnPost()
    {
        
    }
}