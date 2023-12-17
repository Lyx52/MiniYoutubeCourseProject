using Domain.Constants;

namespace Domain.Model.Request;

public class VideoImpressionRequest
{
    public string VideoId { get; set; }
    public ImpressionType Impression { get; set; }
}