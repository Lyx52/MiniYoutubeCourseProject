namespace Domain.Model.View;

public class VideoModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsUnlisted { get; set; }
    public string CreatorId { get; set; }
}