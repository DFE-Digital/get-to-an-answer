namespace Common.Models.ViewModels;

public class ConfigViewModel
{
    public string ContentType => "configuration";

    public string ServiceName => "GetToAnAnswer";
    
    public string ServiceDisplayName => "Get to an answer";

    public BannerPhase Phase { get; init; } = BannerPhase.Beta;

    public string? FeedbackText { get; init; } = string.Empty;

    public string? FeedbackUrl { get; init; } = string.Empty;

    public bool TranslationEnabled { get; init; } = false;

    public bool ShowFooterLinks { get; init; } = true;

    public string LanguageCode { get; init; } = "en";

    public bool IsError { get; init; } = false;

    public enum BannerPhase
    {
        Alpha,
        Beta,
        Live
    }
}