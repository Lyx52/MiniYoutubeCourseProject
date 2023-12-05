using Domain.Model.View;

namespace Domain.Model.Request;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}