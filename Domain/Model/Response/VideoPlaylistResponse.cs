using Domain.Entity;
using Domain.Model.View;

namespace Domain.Model.Response;

public class VideoPlaylistResponse : Response
{
    public IEnumerable<VideoPlaylistModel> Videos { get; set; } = new List<VideoPlaylistModel>();
    public int From { get; set; }
    public int Count { get; set; }
    public int TotalCount { get; set; }
}