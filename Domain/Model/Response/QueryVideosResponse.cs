using Domain.Entity;

namespace Domain.Model.Response;

public class QueryVideosResponse : Response
{
    public IEnumerable<Video> Videos { get; set; } = new List<Video>();
    public int From { get; set; }
    public int Count { get; set; }
    public int TotalCount { get; set; }
}