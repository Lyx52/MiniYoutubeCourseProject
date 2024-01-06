using Domain.Entity;

namespace Domain.Model.Request;

public class RevokeRefreshTokenRequest
{
    public string RefreshToken { get; set; }
    public string Token { get; set; }
}