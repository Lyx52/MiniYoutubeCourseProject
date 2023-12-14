namespace Domain.Model.Request;

public class CreateCommentRequest
{
    public Guid VideoId { get; set; }
    public string Message { get; set; }
}