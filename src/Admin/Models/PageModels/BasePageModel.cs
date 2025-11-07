using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Models.PageModels;

public class BasePageModel() : PageModel
{
    public string? BackLinkSlug { get; set; }

    public bool HasErrors() => ViewData.ModelState is { IsValid: false, ErrorCount: > 0 };
    
    public string? ErrorFor(string key)
    {
        return ViewData.ModelState.ContainsKey(key) && ViewData.ModelState[key]?.Errors.Count > 0
            ? ViewData.ModelState[key]!.Errors[0].ErrorMessage
            : null;
    }
}