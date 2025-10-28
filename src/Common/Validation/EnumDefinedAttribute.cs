namespace Common.Validation;

using System;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class EnumDefinedAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return ValidationResult.Success;
        var type = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
        if (!type.IsEnum) return new ValidationResult("Value is not an enum.");
        return Enum.IsDefined(type, value)
            ? ValidationResult.Success
            : new ValidationResult($"Invalid {type.Name} value.");
    }
}