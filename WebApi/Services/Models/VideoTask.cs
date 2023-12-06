using Domain.Constants;

namespace WebApi.Services.Models;

public class VideoTask
{
    public VideoTaskType Type { get; set; }
    public Guid VideoId { get; set; }
    public Guid WorkSpaceId { get; set; }
}