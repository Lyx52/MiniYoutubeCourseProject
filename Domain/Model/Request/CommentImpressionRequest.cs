using Domain.Constants;

namespace Domain.Model.Request;

public class CommentImpressionRequest
{
    public Guid CommentId { get; set; }
    public ImpressionType Impression { get; set; }
}