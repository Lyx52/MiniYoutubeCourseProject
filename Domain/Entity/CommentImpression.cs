using Domain.Constants;

namespace Domain.Entity;
/**
 *  Id => Composite of UserId and CommentId
 */
public class CommentImpression : IdEntity<string>
{
    public ImpressionType Impression { get; set; }
    public string CommentId { get; set; }
    public Comment Comment { get; set; }
}