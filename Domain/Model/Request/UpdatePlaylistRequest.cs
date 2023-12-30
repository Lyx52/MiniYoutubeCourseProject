namespace Domain.Model.Request;

public class UpdatePlaylistRequest
{
    public Guid PlaylistId { get; set; }
    public string Title { get; set; }
    public List<Guid> Videos { get; set; } = new List<Guid>();
}