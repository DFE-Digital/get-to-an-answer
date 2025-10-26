using Ganss.Xss;
using Markdig;

namespace Common.Custom;

public static class GovUkMarkdown
{
    private static readonly HtmlSanitizer Sanitizer = new HtmlSanitizer();

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static string ToGovUkHtml(string? markdown)
    {
        if (markdown == null)
            return string.Empty;
        
        var html = Markdown.ToHtml(markdown ?? string.Empty, Pipeline);

        // sanitize first
        var safe = Sanitizer.Sanitize(html);

        // naive post-process mappings (extend as needed)
        safe = safe
            .Replace("<p>", "<p class=\"govuk-body\">")
            .Replace("<a ", "<a class=\"govuk-link\" ")
            .Replace("<ul>", "<ul class=\"govuk-list govuk-list--bullet\">")
            .Replace("<ol>", "<ol class=\"govuk-list govuk-list--number\">")
            .Replace("<h1>", "<h1 class=\"govuk-heading-xl\">")
            .Replace("<h2>", "<h2 class=\"govuk-heading-l\">")
            .Replace("<h3>", "<h3 class=\"govuk-heading-m\">");

        return safe;
    }
}