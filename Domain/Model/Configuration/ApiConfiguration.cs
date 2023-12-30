namespace Domain.Model.Configuration;

public class ApiConfiguration
{
    public JWTConfiguration JWT { get; set; }
    public VideoProcessingConfiguration Processing { get; set; }
    public SmtpConfiguration Email { get; set; }
    public EndpointConfig[] Endpoints { get; set; }
    public DatabaseConfiguration Database { get; set; }
}