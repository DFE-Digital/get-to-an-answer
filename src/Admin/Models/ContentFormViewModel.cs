namespace Admin.Models;

public class ContentFormViewModel
{
    public string ContentTitle { get; set; } = string.Empty;

    public string ContentValue { get; set; } = string.Empty;

    public string? NoResultContentValue { get; set; }

    public string? ContentRefName { get; set; }
}
