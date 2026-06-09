using System.ComponentModel.DataAnnotations;

namespace EnterpriseAgentAccelerator.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class NotWhitespaceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && string.IsNullOrWhiteSpace(str))
        {
            return new ValidationResult(
                ErrorMessage ?? $"{validationContext.DisplayName} must not be whitespace.",
                validationContext.MemberName is null ? null : [validationContext.MemberName]);
        }

        return ValidationResult.Success;
    }
}
