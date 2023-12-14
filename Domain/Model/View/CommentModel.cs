namespace Domain.Model.View;

public class CommentModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Message { get; set; }
    public long Likes { get; set; }
    public long Dislikes { get; set; }
    public DateTime Created { get; set; }
}