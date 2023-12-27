namespace Domain.Entity;

public class Playlist : IdEntity<string>
{
    public string CreatorId { get; set; }
    public string Title { get; set; }
    public IEnumerable<Video> Videos { get; set; }
    public IEnumerable<PlaylistVideo> PlaylistsVideos { get; set; } = new List<PlaylistVideo>();
}