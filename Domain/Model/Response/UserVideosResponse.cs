using Domain.Entity;

namespace Domain.Model.Response;

public class UserVideosResponse : Response
{
    public string UserId { get; set; }
    public IEnumerable<Video> Videos { get; set; }
}