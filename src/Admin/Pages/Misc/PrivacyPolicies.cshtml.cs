using Common.Models.PageModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Misc;

[AllowAnonymous]
public class PrivacyPolicies : BasePageModel
{
    public void OnGet()
    {
        
    }
}