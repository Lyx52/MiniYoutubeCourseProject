using Microsoft.AspNetCore.Identity;

namespace Domain.Constants;

public static class PasswordOptionConfig
{
    public static PasswordOptions Default { get; set; } = new PasswordOptions()
    {
        RequireDigit = true,
        RequiredLength = 6,
        RequireLowercase = true,
        RequireUppercase = true,
        RequireNonAlphanumeric = true
    };
}