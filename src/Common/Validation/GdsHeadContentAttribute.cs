namespace Common.Validation;

using System.ComponentModel.DataAnnotations;

public sealed class GdsHeadContentAttribute : ValidationAttribute
{
    // Adjust limits as needed
    public int MinLength { get; init; } = 1;
    public int MaxLength { get; init; } = 500;

    public bool IsRequired { get; init; } = true;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return IsRequired ? new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.") :
                ValidationResult.Success;
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
            return new ValidationResult($"Title must be at least {MinLength} character(s).");
        if (trimmed.Length > MaxLength)
            return new ValidationResult($"Title must be {MaxLength} characters or fewer.");

        return ValidationResult.Success;
    }
}