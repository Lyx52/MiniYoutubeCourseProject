using Domain.Entity;

namespace Domain.Model.Response;

public class UserNotificationResponse : Response
{
    public string UserId { get; set; }
    public IEnumerable<UserNotification> Notifications { get; set; }
}