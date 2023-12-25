namespace Domain.Model.Request;

public class ChangeVideoVisibilityRequest
{
    public Guid VideoId { get; set; }
    public bool IsUnlisted { get; set; }
}