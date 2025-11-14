using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Common.Models.PageModels;

public class BasePageModel : PageModel
{
    [BindProperty(SupportsGet = true)] public string? BackLinkSlug { get; protected set; }
    
    private const string BackLinkTempDataKey = "BackLinkSlugTempData";

    protected virtual ActionResult RedirectToErrorPage() => Redirect("/error");
    
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        TryRestoreBackLinkFromTempData();

        await next();

        SaveBackLinkToTempDataIfPresent();
    }

    private void TryRestoreBackLinkFromTempData()
    {
        if (!string.IsNullOrEmpty(BackLinkSlug))
            return;

        if (TempData.Peek(BackLinkTempDataKey) is string restored && !string.IsNullOrEmpty(restored))
        {
            BackLinkSlug = restored;
        }
    }

    private void SaveBackLinkToTempDataIfPresent()
    {
        if (!string.IsNullOrEmpty(BackLinkSlug))
        {
            TempData[BackLinkTempDataKey] = BackLinkSlug;
        }
    }
    
    public bool HasErrors() => ViewData.ModelState is { IsValid: false, ErrorCount: > 0 };

    public string? ErrorFor(string key)
    {
        return ViewData.ModelState.ContainsKey(key) && ViewData.ModelState[key]?.Errors.Count > 0
            ? ViewData.ModelState[key]!.Errors[0].ErrorMessage
            : null;
    }
}