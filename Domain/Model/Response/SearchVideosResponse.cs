using Domain.Entity;

namespace Domain.Model.Response;

public class SearchVideosResponse : Response
{
    public IEnumerable<Video> Videos { get; set; }
}