using Domain.Constants;

namespace Domain.Entity;

public class Video : IdEntity<string>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string CreatorId { get; set; }
    public string WorkSpaceId { get; set; }
    public IEnumerable<ContentSource>? Sources { get; set; }
    public IEnumerable<Comment> Comments { get; set; }
    public IEnumerable<VideoImpression> Impressions { get; set; }
    public VideoProcessingStatus Status { get; set; }
    public bool IsUnlisted { get; set; }
    public bool NotificationsSent { get; set; }
    public DateTime Created { get; set; }
}