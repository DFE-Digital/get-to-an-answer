using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Common.Validation;

using System.ComponentModel.DataAnnotations;

public sealed class GdsColorAttribute : ValidationAttribute
{
    [StringSyntax("Regex")]
    private const string HexCode = "[#][0-9a-fA-F]{3}([0-9a-fA-F]{3})?";
    
    // Adjust limits as needed
    private int MinLength { get; init; } = 4;
    private int MaxLength { get; init; } = 7;

    public bool IsRequired { get; init; } = true;
    
    private string Pattern { get; init; } = HexCode;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null && IsRequired)
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
        }

        if (value is not string valAsString)
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid data type.");
        
        // Reject whitespace-only strings
        if (string.IsNullOrWhiteSpace(valAsString))
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must not be empty or whitespace.");
        
        // Trim for checks but do not alter model value here
        var trimmed = valAsString.Trim();

        // Length
        if (trimmed.Length < MinLength)
            return new ValidationResult($"{validationContext.DisplayName} must be at least {MinLength} character(s).");
        if (trimmed.Length > MaxLength)
            return new ValidationResult($"{validationContext.DisplayName} must be {MaxLength} characters or fewer.");
        
        // Regex
        Regex regex = new Regex(Pattern);
        
        if (!regex.IsMatch(trimmed))
            return new ValidationResult($"{validationContext.DisplayName} must be a valid hex code");

        return ValidationResult.Success;
    }
}