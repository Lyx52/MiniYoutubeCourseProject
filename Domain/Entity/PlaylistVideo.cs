namespace Domain.Entity;

public class PlaylistVideo : IdEntity<string>
{
    public Video Video { get; set; }
    public string VideoId { get; set; }
    public Playlist Playlist { get; set; }
    public string PlaylistId { get; set; }
}