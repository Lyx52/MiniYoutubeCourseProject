using Domain.Model.Query;
using Domain.Model.View;

namespace Domain.Model.Request;

public class VideoPlaylistRequest
{
    public Guid? CreatorId { get; set; }
    public int From { get; set; }
    public int Count { get; set; }
}