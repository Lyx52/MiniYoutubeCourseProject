namespace Domain.Model.Request;

public class AddOrRemovePlaylistRequest
{
    public IEnumerable<Guid> Videos { get; set; } = new List<Guid>();
    public Guid PlaylistId { get; set; }
    public bool AddVideo { get; set; } = true;
}