using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Home;

public class Index : BasePageModel
{
    public IActionResult OnGet()
    {
        // if logged in redirect to manage questionnaires
        if (User.Identity?.IsAuthenticated == true)
        {
            var cookie = Request.Cookies[TermsOfServiceAgreement.TermsCookieName];
            if (!string.Equals(cookie, "accepted", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(Routes.TermsOfServiceAgreement);           
            }
            
            return Redirect(Routes.QuestionnairesManage);
        }
        return Page();
    }
}