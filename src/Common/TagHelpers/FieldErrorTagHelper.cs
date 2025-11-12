using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Common.TagHelpers;

[HtmlTargetElement("govuk-field-error", Attributes = ForAttributeName)]
public class FieldErrorTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    [HtmlAttributeName("error-id")]
    public string? ErrorId { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    private string GetFullHtmlFieldName(string name)
        => ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var fullName = GetFullHtmlFieldName(For.Name);
        var modelState = ViewContext.ViewData.ModelState;
        var entry = modelState.TryGetValue(fullName, out var value) ? value : null;

        var hasError = entry?.Errors.Count > 0;
        if (!hasError)
        {
            output.SuppressOutput();
            return;
        }

        var message = entry!.Errors[0].ErrorMessage;
        var id = ErrorId ?? $"{fullName}-error";

        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", id);
        output.Attributes.SetAttribute("class", "govuk-error-message");
        output.Content.SetHtmlContent($"""<span class="govuk-visually-hidden">Error:</span> {message}""");
    }
}