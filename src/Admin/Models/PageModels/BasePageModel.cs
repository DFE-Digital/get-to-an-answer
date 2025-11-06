using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Models.PageModels;

public class BasePageModel : PageModel
{
    public enum BannerPhase
    {
        Alpha,
        Beta,
        Live
    }

    public string? ErrorFor(string key)
    {
        return ViewData.ModelState.ContainsKey(key) && ViewData.ModelState[key]?.Errors.Count > 0
            ? ViewData.ModelState[key]!.Errors[0].ErrorMessage
            : null;
    }

    //TODO: Potentially move this to config or service if not needed across all pages
    public static string ContentType { get; } = "configuration";

    public string ServiceName { get; set; } = "GetToAnAnswer";

    public BannerPhase Phase { get; set; } = BannerPhase.Beta;

    public string? FeedbackText { get; set; } = string.Empty;

    public string? FeedbackUrl { get; set; } = string.Empty;

    public bool TranslationEnabled { get; set; } = false;

    public bool ShowFooterLinks { get; set; } = true;

    public string LanguageCode { get; set; } = "en";

    public bool IsError { get; set; } = false;
}