namespace Domain.Model.View;

public class GetVideoPlaylistModel
{
    public int From { get; set; }
    public int Count { get; set; }
    public Guid? CreatorId { get; set; } = null;
    public Guid? PlaylistId { get; set; } = null;
    public bool OrderByNewest { get; set; } = false;
}