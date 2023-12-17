using Domain.Constants;

namespace Domain.Entity;
/**
 *  Id => Composite of UserId and VideoId
 */
public class VideoImpression : IdEntity<string>
{
    public ImpressionType Impression { get; set; }
    public string VideoId { get; set; }
    public Video Video { get; set; }
}