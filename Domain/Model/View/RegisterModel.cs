using System.ComponentModel.DataAnnotations;
using Domain.Attributes;

namespace Domain.Model;

public class RegisterModel
{
    [Required]
    public string Username { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [PasswordValidation(6, true, true, true, true)]
    public string Password { get; set; }
    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }
}