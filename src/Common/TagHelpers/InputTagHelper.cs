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

        // Normalize to a safe id (dots -> dashes or underscores)
        var baseId = fullName.Replace(".", "-").Replace("[", "-").Replace("]", "");

        // If the element already has an id attribute, use that as the base instead
        if (output.Attributes.TryGetAttribute("id", out var existingIdAttr))
        {
            baseId = existingIdAttr.Value?.ToString() ?? baseId;
        }

        // Determine whether ModelState contains an error for this field
        var modelState = ViewContext.ViewData.ModelState;
        var hasError = modelState.TryGetValue(fullName, out var entry) && entry.Errors.Count > 0;

        // If there is an error, append -field-error to the input id
        var finalId = hasError ? $"{baseId}-field-error" : baseId;

        // Set or replace the id attribute on the element
        output.Attributes.SetAttribute("id", finalId);

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

        // Ensure the element has the base govuk-input class (or govuk-select / govuk-textarea as appropriate)
        // We keep it simple: prefer govuk-input for input elements, leave existing classes otherwise.
        if (string.Equals(output.TagName, "input", StringComparison.OrdinalIgnoreCase))
        {
            if (output.Attributes.TryGetAttribute("class", out var existingClassAttr))
            {
                var classes = GetAttributeStringValue(existingClassAttr);
                var parts = classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!parts.Contains("govuk-input"))
                {
                    parts.Insert(0, "govuk-input"); // ensure govuk-input is present first
                }

                output.Attributes.SetAttribute("class", string.Join(" ", parts));
            }
            else
            {
                output.Attributes.SetAttribute("class", "govuk-input");
            }
        }
        else if (string.Equals(output.TagName, "select", StringComparison.OrdinalIgnoreCase))
        {
            // for selects prefer govuk-select
            if (output.Attributes.TryGetAttribute("class", out var existingClassAttr))
            {
                var classes = GetAttributeStringValue(existingClassAttr);
                var parts = classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!parts.Contains("govuk-select"))
                {
                    parts.Insert(0, "govuk-select");
                }

                output.Attributes.SetAttribute("class", string.Join(" ", parts));
            }
            else
            {
                output.Attributes.SetAttribute("class", "govuk-select");
            }
        }
        else if (string.Equals(output.TagName, "textarea", StringComparison.OrdinalIgnoreCase))
        {
            // for textareas prefer govuk-textarea
            if (output.Attributes.TryGetAttribute("class", out var existingClassAttr))
            {
                var classes = GetAttributeStringValue(existingClassAttr);
                var parts = classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!parts.Contains("govuk-textarea"))
                {
                    parts.Insert(0, "govuk-textarea");
                }

                output.Attributes.SetAttribute("class", string.Join(" ", parts));
            }
            else
            {
                output.Attributes.SetAttribute("class", "govuk-textarea");
            }
        }

        // If there's an error, ensure aria-describedby references the error id (and keep any existing describedby)
        if (hasError)
        {
            // Ensure the error id is first in aria-describedby so screen readers announce it before hints
            if (output.Attributes.TryGetAttribute("aria-describedby", out var describedByAttr)
                && !string.IsNullOrWhiteSpace(GetAttributeStringValue(describedByAttr)))
            {
                var existing = GetAttributeStringValue(describedByAttr);
                // Avoid duplicates and preserve order (error id first)
                var parts = existing.Split(' ').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
                if (!parts.Contains(finalId))
                {
                    parts.Insert(0, finalId);
                }

                output.Attributes.SetAttribute("aria-describedby", string.Join(" ", parts));
            }
            else
            {
                output.Attributes.SetAttribute("aria-describedby", finalId);
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
                // If there was no class previously, ensure a govuk base class exists for inputs
                var baseClass = string.Equals(output.TagName, "select", StringComparison.OrdinalIgnoreCase)
                    ?
                    "govuk-select"
                    :
                    string.Equals(output.TagName, "textarea", StringComparison.OrdinalIgnoreCase)
                        ? "govuk-textarea"
                        : "govuk-input";
                output.Attributes.SetAttribute("class", $"{baseClass} govuk-input--error");
            }
        }
    }
// ... existing code ...
}