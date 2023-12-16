namespace Domain.Model.View;

public class VideoQuery
{
    public int From { get; set; }
    public int Count { get; set; }
    public Guid CreatorId { get; set; }
    public bool OrderByCreated { get; set; }
    public bool IncludeUnlisted { get; set; }
}