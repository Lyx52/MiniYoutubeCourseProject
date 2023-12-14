namespace Domain.Entity;

public class Comment : IdEntity<string>
{
    public string Message { get; set; }
    public string CommenterId { get; set; }
    public Video Video { get; set; }
    public string VideoId { get; set; }
    public long Likes { get; set; }
    public long Dislikes { get; set; }
    public DateTime Created { get; set; }
}