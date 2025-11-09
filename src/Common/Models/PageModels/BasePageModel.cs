using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Common.Models.PageModels;

public class BasePageModel : PageModel
{
    public string? ErrorFor(string key)
    {
        return ViewData.ModelState.ContainsKey(key) && ViewData.ModelState[key]?.Errors.Count > 0
            ? ViewData.ModelState[key]!.Errors[0].ErrorMessage
            : null;
    }
}