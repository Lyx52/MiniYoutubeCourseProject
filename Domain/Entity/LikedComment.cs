namespace Domain.Entity;

public class LikedComment : IdEntity<string>
{
    public string UserId { get; set; }
    public string CommentId { get; set; }
    public bool IsLike { get; set; }
}