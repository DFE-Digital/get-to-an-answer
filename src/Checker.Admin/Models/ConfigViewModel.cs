namespace Checker.Admin.Models;

public class ConfigViewModel
{
    
    public enum BannerPhase
    {
        Alpha,
        Beta,
        Live
    }
    
    public static string ContentType { get; } = "configuration";
    
    public string ServiceName { get; set; } = "Checker";

    public BannerPhase Phase { get; set; } = BannerPhase.Beta;

    public string? FeedbackText { get; set; } = string.Empty;

    public string? FeedbackUrl { get; set; } = string.Empty;
    
    public bool TranslationEnabled { get; set; } = false;
    
    public bool ShowFooterLinks { get; set; } = true;

    public string LanguageCode { get; set; } = "en";
}