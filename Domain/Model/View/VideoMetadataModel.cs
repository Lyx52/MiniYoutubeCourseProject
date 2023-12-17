using Domain.Constants;

namespace Domain.Model.View;

public class VideoMetadataModel
{
    public Guid VideoId { get; set; }
    public Guid CreatorId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public long Likes { get; set; }
    public long Dislikes { get; set; }
    public ImpressionType UserImpression { get; set; }
}