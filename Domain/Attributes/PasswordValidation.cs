using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Domain.Attributes;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public class PasswordValidation : ValidationAttribute
{
    public PasswordOptions Options { get; set; }
    private StringBuilder _errorBuilder;
    public PasswordValidation(int requiredLength, bool requireDigit, bool requireNonAlphanumeric, bool requireLowercase, bool requireUppercase)
    {
        Options = new PasswordOptions()
        {
            RequiredLength = requiredLength,
            RequireDigit = requireDigit,
            RequireNonAlphanumeric = requireNonAlphanumeric,
            RequireLowercase = requireLowercase,
            RequireUppercase = requireUppercase,
        };
        _errorBuilder = new StringBuilder();
    }

    public override bool IsValid(object? value)
    {
        string? password = Convert.ToString(value, CultureInfo.CurrentCulture);
        bool isValid = true;
        if (string.IsNullOrWhiteSpace(password) || password.Length < Options.RequiredLength)
        {
            _errorBuilder.AppendLine("Password length must be 6 symbols or longer");
            isValid = false;
        }
        if (Options.RequireNonAlphanumeric && password.All(IsLetterOrDigit))
        {
            _errorBuilder.AppendLine("Password must have non alphanumeric character");
            isValid = false;
        }
        if (Options.RequireDigit && !password.Any(IsDigit))
        {
            _errorBuilder.AppendLine("Password must have a digit");
            isValid = false;
        }
        if (Options.RequireLowercase && !password.Any(IsLower))
        {
            _errorBuilder.AppendLine("Password must have a lowercase character");
            isValid = false;
        }
        if (Options.RequireUppercase && !password.Any(IsUpper))
        {
            _errorBuilder.AppendLine("Password must have a uppercase character");
            isValid = false;
        }
        return isValid;
    }

    public override string FormatErrorMessage(string name)
    {

        return _errorBuilder.ToString();
    }

    private bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }
    
    private bool IsLower(char c)
    {
        return c is >= 'a' and <= 'z';
    }
    private bool IsUpper(char c)
    {
        return c is >= 'A' and <= 'Z';
    }
    private bool IsLetterOrDigit(char c)
    {
        return IsUpper(c) || IsLower(c) || IsDigit(c);
    }
}