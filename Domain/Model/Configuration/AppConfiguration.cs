namespace Domain.Model.Configuration;

public class AppConfiguration
{
    public string ApiEndpoint { get; set; }
    public JWTConfiguration JWT { get; set; }
}