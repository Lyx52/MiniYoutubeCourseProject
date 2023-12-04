namespace Domain.Model.Response;

public class LoginResponseModel : Response
{
    public string Token { get; set; }
    public string BearerToken { get; set; }
    public DateTime Expiration { get; set; }
}