namespace Domain.Model.Request;

public class CreatePlaylistRequest
{
    public string Title { get; set; }
    public List<Guid> Videos { get; set; } = new List<Guid>();
}