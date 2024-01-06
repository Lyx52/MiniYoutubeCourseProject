namespace Domain.Tests;

public class SharedAuthState
{
    public string BearerToken { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public Guid WorkSpaceId { get; set; }
    public Guid VideoId { get; set; }
    public Guid PlaylistId { get; set; }
    public string UserId { get; set; }
}