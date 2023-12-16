using Domain.Model.View;

namespace Domain.Model.Request;

public class VideoPlaylistRequest
{
    public VideoQuery Query { get; set; }
}