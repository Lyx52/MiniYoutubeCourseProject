namespace Domain.Model.Response;

public class RefreshTokenResponse : Response
{
    public string Token { get; set; }
    public string BearerToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
}