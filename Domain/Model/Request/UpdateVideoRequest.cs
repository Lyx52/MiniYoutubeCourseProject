namespace Domain.Model.Request;

public class UpdateVideoRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsUnlisted { get; set; }
    public string VideoId { get; set; }
}