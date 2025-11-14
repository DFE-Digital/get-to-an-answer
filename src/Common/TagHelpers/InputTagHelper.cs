using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Common.TagHelpers;

// Targets inputs/selects/textareas that are bound with asp-for
[HtmlTargetElement("input", Attributes = ForAttributeName)]
[HtmlTargetElement("select", Attributes = ForAttributeName)]
[HtmlTargetElement("textarea", Attributes = ForAttributeName)]
public class FieldInputTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; } = default!;

    // Gives access to ModelState / TemplateInfo
    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; } = default!;
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
{
    // Determine the model field full name (e.g. "Parent.Child.Name")
    var fullName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
    
    // Determine whether ModelState contains an error for this field
    var modelState = ViewContext.ViewData.ModelState;
    var hasError = modelState.TryGetValue(fullName, out var entry) && entry.Errors.Count > 0;

    if (fullName.Contains('.'))
    {
        fullName = fullName.Substring(fullName.LastIndexOf('.') + 1);
    }
    
    // Helper to get the string value from a TagHelper attribute (handles IHtmlContent)
    static string GetAttributeStringValue(TagHelperAttribute? attr)
    {
        if (attr == null || attr.Value == null)
            return string.Empty;

        if (attr.Value is Microsoft.AspNetCore.Html.IHtmlContent htmlContent)
        {
            using var sw = new System.IO.StringWriter();
            htmlContent.WriteTo(sw, System.Text.Encodings.Web.HtmlEncoder.Default);
            return sw.ToString();
        }

        return attr.Value.ToString() ?? string.Empty;
    }

    // If there's an error, ensure aria-describedby includes the error id (and keep any existing describedby)
    if (hasError)
    {
        // Convention: error tag helper uses "{fullName}-error" (e.g. "Accepted-error")
        var errorId = $"{fullName.ToLower()}-field-error";

        if (output.Attributes.TryGetAttribute("aria-describedby", out var describedByAttr)
            && !string.IsNullOrWhiteSpace(GetAttributeStringValue(describedByAttr)))
        {
            var existing = GetAttributeStringValue(describedByAttr);
            var parts = existing.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!parts.Contains(errorId))
            {
                // Put error id first so SRs announce it before hints
                parts.Insert(0, errorId);
            }
            output.Attributes.SetAttribute("aria-describedby", string.Join(" ", parts));
        }
        else
        {
            output.Attributes.SetAttribute("aria-describedby", errorId);
        }

        // Add an error-specific CSS class if not present
        if (output.Attributes.TryGetAttribute("class", out var classAttr))
        {
            var classes = GetAttributeStringValue(classAttr);
            var classParts = classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!classParts.Contains("govuk-input--error"))
            {
                classParts.Add("govuk-input--error");
                output.Attributes.SetAttribute("class", string.Join(" ", classParts));
            }
        }
        else
        {
            // If there was no class previously, choose a sensible default base class
            var baseClass = string.Equals(output.TagName, "select", StringComparison.OrdinalIgnoreCase)
                ? "govuk-select"
                : string.Equals(output.TagName, "textarea", StringComparison.OrdinalIgnoreCase)
                    ? "govuk-textarea"
                    : "govuk-input";

            output.Attributes.SetAttribute("class", $"{baseClass} govuk-input--error");
        }
    }
}
// ... existing code ...
}