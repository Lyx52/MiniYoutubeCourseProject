using Domain.Constants;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Models;

public class VideoTask : BackgroundTask
{
    public Guid VideoId { get; set; }
    public Guid WorkSpaceId { get; set; }
}