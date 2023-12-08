using System.ComponentModel.DataAnnotations;
using Domain.Attributes;

namespace Domain.Model.View;

public class LoginModel
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}