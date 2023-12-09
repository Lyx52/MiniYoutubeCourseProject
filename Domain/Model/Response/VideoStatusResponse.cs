using Domain.Constants;

namespace Domain.Model.Response;

public class VideoStatusResponse : Response
{
    public Guid VideoId { get; set; }
    public VideoProcessingStatus Status { get; set; }
}