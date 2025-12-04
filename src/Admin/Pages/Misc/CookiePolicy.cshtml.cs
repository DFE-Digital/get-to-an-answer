using Admin.Models;
using Common.Models.PageModels;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Admin.Pages.Misc;

public class CookiePolicy : BasePageModel
{
    public IActionResult OnGet()
    {
        var consent = HttpContext.Features.Get<ITrackingConsentFeature>() ??
                      throw new InvalidOperationException("ITrackingConsentFeature is not available.");

        AcceptCookies = consent.CanTrack;

        return Page();
    }
    
    public IActionResult OnPostConsent(
        [FromServices] IOptions<CookiePolicyOptions> cookiePolicyOptions)
    {
        var cookieOptions = cookiePolicyOptions.Value.ConsentCookie.Build(HttpContext);
        var cookieValue = AcceptCookies ? cookiePolicyOptions.Value.ConsentCookieValue : "no";

        HttpContext.Response.Cookies.Append(
            cookiePolicyOptions.Value.ConsentCookie.Name ?? string.Empty,
            cookieValue,
            cookieOptions);

        ShowSuccessBanner = true;
        
        return Page();
    }
}