using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Common.TagHelpers;

[HtmlTargetElement("govuk-form-group", Attributes = ForAttributeName)]
public class GovUkFormGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; } = default!;

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; } = default!;

    [HtmlAttributeName("specific-id-for-form-group-error")]
    public string? SpecificIdForFormGroupError { get; set; }

    private string GetFullHtmlFieldName(string name) => ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var fullName = GetFullHtmlFieldName(For.Name);
        var modelState = ViewContext.ViewData.ModelState;

        var hasError = false;

        if (!string.IsNullOrEmpty(SpecificIdForFormGroupError) 
            && modelState.ContainsKey(SpecificIdForFormGroupError) 
            && modelState[SpecificIdForFormGroupError]?.Errors.Count > 0 
            || modelState.TryGetValue(fullName, out var entry) && entry.Errors.Count > 0)
        {
            hasError = true;
        }


        var css = "govuk-form-group" + (hasError ? " govuk-form-group--error" : "");

        if (output.Attributes.TryGetAttribute("class", out var existing))
        {
            output.Attributes.SetAttribute("class", $"{existing.Value} {css}".Trim());
        }
        else
        {
            output.Attributes.SetAttribute("class", css);
        }
    }
}