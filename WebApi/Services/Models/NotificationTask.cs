namespace WebApi.Services.Models;

public class NotificationTask : BackgroundTask
{
    public Guid VideoId { get; set; }
}