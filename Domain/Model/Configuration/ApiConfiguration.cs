namespace Domain.Model.Configuration;

public class ApiConfiguration
{
    public JWTConfiguration JWT { get; set; }
    public VideoProcessingConfiguration Processing { get; set; }
}