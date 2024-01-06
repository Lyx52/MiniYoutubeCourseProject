namespace Domain.Model.Request;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
    public string ExpiredToken { get; set; }
}