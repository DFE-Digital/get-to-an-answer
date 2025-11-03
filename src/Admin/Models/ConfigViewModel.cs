using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Models;

public class ConfigViewModel : PageModel
{
    public enum BannerPhase
    {
        Alpha,
        Beta,
        Live
    }
    
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