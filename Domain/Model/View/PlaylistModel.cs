namespace Domain.Model.View;

public class PlaylistModel
{
    public Guid PlaylistId { get; set; }
    public Guid CreatorId { get; set; }
    public string Title { get; set; }
}