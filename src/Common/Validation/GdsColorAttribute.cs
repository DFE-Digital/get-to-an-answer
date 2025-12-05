using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Common.Validation;

using System.ComponentModel.DataAnnotations;

public sealed class GdsColorAttribute : ValidationAttribute
{
    [StringSyntax("Regex")]
    private const string HexCode = "[#][0-9a-fA-F]{3,8}";
    
    // Adjust limits as needed
    private int[] LengthsAllowed { get; init; } = [
        4, // e.g. #fff
        5, // e.g. #000A
        7, // e.g. #0000AA
        9 // e.g. #000000AA
    ];

    public bool IsRequired { get; init; } = true;
    
    private string Pattern { get; init; } = HexCode;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var friendlyName = ToFriendlyLabel(validationContext.DisplayName);
        
        if (value is null && IsRequired)
        {
            return new ValidationResult(ErrorMessage ?? $"{friendlyName} is required.");
        }

        if (value is not string valAsString)
            return new ValidationResult(ErrorMessage ?? $"{friendlyName} must be a valid data type.");
        
        // Reject whitespace-only strings
        if (string.IsNullOrWhiteSpace(valAsString))
            return new ValidationResult(ErrorMessage ?? $"{friendlyName} must not be empty or whitespace.");
        
        // Trim for checks but do not alter model value here
        var trimmed = valAsString.Trim();

        // Length
        if (!LengthsAllowed.Contains(trimmed.Length))
            return new ValidationResult($"{friendlyName} must be one of these lengths: {string.Join(", ", LengthsAllowed)}.");
        
        // Regex
        var regex = new Regex(Pattern);
        
        if (!regex.IsMatch(trimmed))
            return new ValidationResult($"{friendlyName} must be a valid hex code");

        return ValidationResult.Success;
    }
    
    private static readonly Regex PascalCaseWordBoundaryRegex =
        new ("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", RegexOptions.Compiled);

    private static string ToFriendlyLabel(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // 1. Split PascalCase: TextColor -> Text Color, ErrorMessageColor -> Error Message Color
        var withSpaces = PascalCaseWordBoundaryRegex.Replace(input, " $1");

        // 2. Lower‑case all except first word
        var words = withSpaces.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return input;

        words[0] = CapitaliseFirst(words[0]);
        for (var i = 1; i < words.Length; i++)
        {
            words[i] = words[i].ToLowerInvariant();
        }

        var result = string.Join(' ', words);

        // 3. US -> UK spellings
        result = result.Replace("Color", "colour", StringComparison.OrdinalIgnoreCase);

        return result;
    }

    private static string CapitaliseFirst(string s)
    {
        if (s.Length == 0) return s;
        if (s.Length == 1) return char.ToUpperInvariant(s[0]).ToString();
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }
}