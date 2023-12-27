using Domain.Model.View;

namespace Domain.Model.Response;

public class CreatorPlaylistsResponse : Response
{
    public Guid CreatorId { get; set; }
    public IEnumerable<PlaylistModel> Playlists { get; set; }
}