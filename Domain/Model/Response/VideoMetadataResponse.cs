using Domain.Constants;
using Domain.Model.View;

namespace Domain.Model.Response;

public class VideoMetadataResponse : Response
{
    public VideoMetadataModel Metadata { get; set; }
}