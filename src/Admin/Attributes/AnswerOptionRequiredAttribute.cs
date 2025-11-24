using System.ComponentModel.DataAnnotations;
using Admin.Models;

namespace Admin.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AnswerOptionRequiredAttribute(string fieldType) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var isMissing = value switch
        {
            Enum e => Convert.ToInt32(e) == 0,
            string s => string.IsNullOrWhiteSpace(s),
            null => true,
            _ => false
        };
        
        if (!isMissing)
            return ValidationResult.Success;

        if (validationContext.ObjectInstance is not AnswerOptionsViewModel option)
            return ValidationResult.Success;

        var label = fieldType switch
        {
            "content" => "content",
            "hint" => "hint text",
            "destination" => "destination",
            _ => fieldType
        };

        
        var message = $"Option {option.OptionNumber} {label} is required";

        return new ValidationResult(message, [validationContext.MemberName!]);
    }
}