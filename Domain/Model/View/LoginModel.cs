using System.ComponentModel.DataAnnotations;
using Domain.Attributes;

namespace Domain.Model.View;

public class LoginModel
{
    [Required]
    public string Username { get; set; }
    [Required]
    [PasswordValidation(6, true, true, true, true)]
    public string Password { get; set; }
}