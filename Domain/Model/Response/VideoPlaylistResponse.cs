using Domain.Model.View;

namespace Domain.Model.Response;

public class VideoPlaylistResponse : Response
{
    public IEnumerable<VideoPlaylistModel> Videos { get; set; }
}