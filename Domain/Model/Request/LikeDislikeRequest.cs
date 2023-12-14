namespace Domain.Model.Request;

public class LikeDislikeRequest
{
    public string CommentId { get; set; }
    public bool IsLike { get; set; }
}