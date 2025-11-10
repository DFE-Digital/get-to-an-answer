using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Home;

public class TermsOfServiceAgreement : BasePageModel
{
    [BindProperty]
    public bool Accepted { get; set; }
    
    public void OnGet()
    {
        
    }
    
    public IActionResult OnPost()
    {
        if (!Accepted)
            ModelState.AddModelError(nameof(Accepted), "You need to accept the agreement to continue");
        
        if (!ModelState.IsValid)
            return Page();
        
        return Redirect(Routes.QuestionnairesManage);
    }
}