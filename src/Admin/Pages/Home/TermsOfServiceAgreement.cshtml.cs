using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Pages.Home;

[AllowAnonymous]
public class TermsOfServiceAgreement : BasePageModel
{
    [BindProperty(Name = "Accepted")]
    public bool Accepted { get; set; }
    
    public IActionResult OnGet()
    {
        if (HasAcceptedTerms())
        {
            return Redirect(Routes.QuestionnairesManage);
        }
        
        return Page();
    }
    
    public IActionResult OnPost()
    {
        if (!Accepted)
            ModelState.AddModelError(nameof(Accepted), "You need to accept the agreement to continue");
        
        if (!ModelState.IsValid)
            return Page();
        
        if (Accepted)
            SetTermsAcceptedCookie();
        
        return Redirect(Routes.QuestionnairesManage);
    }
    
    public static readonly string TermsCookieName = "AcceptedAgreement";
    private const int TermsCookieDays = 365;

    private bool HasAcceptedTerms()
    {
        var cookie = Request.Cookies[TermsCookieName];
        return string.Equals(cookie, "accepted", StringComparison.OrdinalIgnoreCase);
    }

    private void SetTermsAcceptedCookie()
    {
        var options = new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(TermsCookieDays),
            Secure = true,              // send only over HTTPS
            HttpOnly = false,           // allow client-side checks if needed
            SameSite = SameSiteMode.Lax,
            Path = "/"                  // available to entire site
        };

        Response.Cookies.Append(TermsCookieName, "accepted", options);
    }
}