namespace Domain.Model.Response;

public class LoginResponse : Response
{
    public string Token { get; set; }
    public string BearerToken { get; set; }
    public DateTime Expiration { get; set; }
}