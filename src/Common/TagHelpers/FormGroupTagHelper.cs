using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Common.TagHelpers;

[HtmlTargetElement("govuk-form-group", Attributes = ForAttributeName)]
public class FormGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; } = default!;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    private string GetFullHtmlFieldName(string name) => ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var fullName = GetFullHtmlFieldName(For.Name);
        var modelState = ViewContext.ViewData.ModelState;
        var hasError = modelState.ContainsKey(fullName) && modelState[fullName]?.Errors.Count > 0;

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