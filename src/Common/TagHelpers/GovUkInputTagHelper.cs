using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Common.TagHelpers;

// Targets inputs/selects/textareas that are bound with asp-for
[HtmlTargetElement("govuk-input", Attributes = ForAttributeName)]
public class GovUkInputTagHelper(IHtmlGenerator generator) : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; } = default!;

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Generate a standard MVC input (equivalent to <input asp-for="...">)
        var tagBuilder = generator.GenerateTextBox(
            ViewContext,
            For.ModelExplorer,
            For.Name,
            For.Model,
            format: null,
            htmlAttributes: null);

        // Start with the generated input as the base
        output.TagName = "input";
        output.TagMode = TagMode.SelfClosing;

        // Copy generated attributes into TagHelperOutput (name, id, value, etc.)
        output.Attributes.Clear();
        foreach (var attr in tagBuilder.Attributes)
        {
            output.Attributes.Add(attr.Key, attr.Value);
        }

        // Merge any attributes defined in the Razor markup (id, class, aria-describedby, etc.)
        foreach (var attr in context.AllAttributes)
        {
            // Skip asp-* attributes; they are not real HTML attributes
            if (attr.Name.StartsWith("asp-", System.StringComparison.OrdinalIgnoreCase))
                continue;

            // If attribute already exists, let the Razor-specified value win
            output.Attributes.SetAttribute(attr.Name, attr.Value);
        }

        // Helper to get the string value from a TagHelper attribute (handles IHtmlContent)
        static string GetAttributeStringValue(TagHelperAttribute? attr)
        {
            if (attr == null || attr.Value == null)
                return string.Empty;

            if (attr.Value is IHtmlContent htmlContent)
            {
                using var sw = new StringWriter();
                htmlContent.WriteTo(sw, System.Text.Encodings.Web.HtmlEncoder.Default);
                return sw.ToString();
            }

            return attr.Value.ToString() ?? string.Empty;
        }

        // Ensure type="text" unless explicitly overridden
        if (!output.Attributes.TryGetAttribute("type", out _))
        {
            output.Attributes.SetAttribute("type", "text");
        }

        // Ensure the govuk-input base class is present (or preserve checkbox class if used that way)
        if (output.Attributes.TryGetAttribute("class", out var classAttr))
        {
            var existingClasses = GetAttributeStringValue(classAttr);
            var parts = existingClasses.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!parts.Contains("govuk-input") && !parts.Contains("govuk-checkboxes__input"))
            {
                parts.Insert(0, "govuk-input");
            }

            output.Attributes.SetAttribute("class", string.Join(" ", parts));
        }
        else
        {
            output.Attributes.SetAttribute("class", "govuk-input");
        }

        // === Error handling / aria-describedby logic ===

        var fullName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        var modelState = ViewContext.ViewData.ModelState;
        var hasError = modelState.TryGetValue(fullName, out var entry) && entry.Errors.Count > 0;

        var simpleName = fullName.Contains('.')
            ? fullName[(fullName.LastIndexOf('.') + 1)..]
            : fullName;

        if (hasError)
        {
            var errorId = $"{simpleName.ToLower()}-field-error";

            // Ensure aria-describedby includes the error id (and keep any existing describedby)
            if (output.Attributes.TryGetAttribute("aria-describedby", out var describedByAttr)
                && !string.IsNullOrWhiteSpace(GetAttributeStringValue(describedByAttr)))
            {
                var existing = GetAttributeStringValue(describedByAttr);
                var parts = existing.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!parts.Contains(errorId))
                {
                    parts.Insert(0, errorId);
                }

                output.Attributes.SetAttribute("aria-describedby", string.Join(" ", parts));
            }
            else
            {
                output.Attributes.SetAttribute("aria-describedby", errorId);
            }

            // Add an error-specific CSS class if not present
            if (output.Attributes.TryGetAttribute("class", out var errClassAttr))
            {
                var classes = GetAttributeStringValue(errClassAttr);
                var classParts = classes.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!classParts.Contains("govuk-input--error"))
                {
                    classParts.Add("govuk-input--error");
                    output.Attributes.SetAttribute("class", string.Join(" ", classParts));
                }
            }
            else
            {
                output.Attributes.SetAttribute("class", "govuk-input govuk-input--error");
            }
        }
    }
}