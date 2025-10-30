using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FileToApi.Attributes;

public class ValidUsernameAttribute : ValidationAttribute
{
    private static readonly Regex UsernameRegex = new Regex(
        @"^[a-zA-Z0-9._@\-\\]+$",
        RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult("Username is required");
        }

        var username = value.ToString()!;

        if (username.Length < 2 || username.Length > 256)
        {
            return new ValidationResult("Username must be between 2 and 256 characters");
        }

        if (!UsernameRegex.IsMatch(username))
        {
            return new ValidationResult("Username contains invalid characters. Only letters, numbers, dots, underscores, @, hyphens, and backslashes are allowed");
        }

        return ValidationResult.Success;
    }
}
