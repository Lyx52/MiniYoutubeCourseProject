using Domain.Constants;

namespace Domain.Model.Request;

public class CommentImpressionRequest
{
    public string CommentId { get; set; }
    public ImpressionType Impression { get; set; }
}