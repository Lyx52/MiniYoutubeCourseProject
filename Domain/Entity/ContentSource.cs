using Domain.Constants;

namespace Domain.Entity;

public class ContentSource : IdEntity<string>
{
    public Video Video { get; set; }
    public string VideoId { get; set; }
    public string ContentType { get; set; }
    public string Resolution { get; set; }
    public ContentSourceType Type { get; set; }
}