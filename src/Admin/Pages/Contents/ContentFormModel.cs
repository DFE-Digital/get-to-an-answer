namespace Admin.Pages.Contents;

public record ContentFormModel(
    string? QuestionnaireTitle,
    string? BackLinkSlug,
    string? ContentTitle,
    string? ContentValue,
    bool ShowResultsSummary,
    string? ContentRefName,
    string ResultsSummaryHint,
    string SaveHandler,
    bool ShowDelete);
