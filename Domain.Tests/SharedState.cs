namespace Domain.Tests;

public class SharedAuthState
{
    public string BearerToken { get; set; }
    public string Token { get; set; }
    public Guid WorkSpaceId { get; set; }
    public Guid VideoId { get; set; }
    public string UserId { get; set; }
}