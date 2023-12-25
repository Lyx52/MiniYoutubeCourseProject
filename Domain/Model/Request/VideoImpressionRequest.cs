using Domain.Constants;

namespace Domain.Model.Request;

public class VideoImpressionRequest
{
    public Guid VideoId { get; set; }
    public ImpressionType Impression { get; set; }
}