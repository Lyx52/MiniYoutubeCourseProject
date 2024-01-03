using System.ComponentModel.DataAnnotations;
using Domain.Attributes;

namespace Domain.Model;

public class RegisterModel
{
    [Required(ErrorMessage = "'Username' is required")]
    public string Username { get; set; }
    [Required(ErrorMessage = "'Email' is required")]
    [EmailAddress(ErrorMessage = "'Email' is invalid")]
    public string Email { get; set; }
    [Required(ErrorMessage = "'Password' is required")]
    [PasswordValidation(6, true, true, true, true)]
    public string Password { get; set; }
    [Required(ErrorMessage = "'Confirm password' is required")]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }
}