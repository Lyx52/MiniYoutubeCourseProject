using Domain.Constants;
using Domain.Model.Configuration;

namespace Domain.Model.Request;

public class QueryVideosRequest
{
    public string? Title { get; set; }
    public Guid? CreatorId { get; set; }
    public bool QueryUserVideos { get; set; }
    public int From { get; set; }
    public int Count { get; set; }
    public VideoProcessingStatus? Status { get; set; }
}