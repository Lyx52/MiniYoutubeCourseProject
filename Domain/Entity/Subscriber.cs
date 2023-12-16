namespace Domain.Entity;

public class Subscriber : IdEntity<string>
{
    public string CreatorId { get; set; }
    public string SubscriberId { get; set; }
}