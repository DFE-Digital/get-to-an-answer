namespace Common.Validation;

using System.ComponentModel.DataAnnotations;

public sealed class GdsTitleAttribute : ValidationAttribute
{
    // Adjust limits as needed
    public int MinLength { get; init; } = 1;
    public int MaxLength { get; init; } = 500;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var s = value as string;

        // Required
        if (string.IsNullOrWhiteSpace(s))
            return new ValidationResult("Title is required and cannot be blank or whitespace.");

        // Trim for checks but do not alter model value here
        var trimmed = s.Trim();

        // Length
        if (trimmed.Length < MinLength)
            return new ValidationResult($"Title must be at least {MinLength} character(s).");
        if (trimmed.Length > MaxLength)
            return new ValidationResult($"Title must be {MaxLength} characters or fewer.");

        // No all-caps (allow short words like A, or words containing numbers/emojis/mixed)
        // Consider text 'all caps' if it has at least 2 letters and all letters are uppercase.
        var letters = trimmed.Where(char.IsLetter).ToArray();
        if (letters.Length >= 2 && letters.All(char.IsUpper))
            return new ValidationResult("Do not use all capitals.");

        // No repeated spaces
        for (int i = 1; i < trimmed.Length; i++)
        {
            if (char.IsWhiteSpace(trimmed[i]) && char.IsWhiteSpace(trimmed[i - 1]))
                return new ValidationResult("Avoid repeated spaces.");
        }

        // Control characters (other than standard whitespace) not allowed
        foreach (var ch in trimmed)
        {
            if (char.IsControl(ch) && !char.IsWhiteSpace(ch))
                return new ValidationResult("Control characters are not allowed.");
        }

        return ValidationResult.Success;
    }
}