namespace Domain.Entity;

public class UserNotification : IdEntity<string>
{
    public string Message { get; set; }
    public string RedirectLink { get; set; }
    public string UserId { get; set; }
}