using Domain.Constants;

namespace Domain.Model.View;

public class ContentSourceModel
{
    public string Id { get; set; }
    public string ContentType { get; set; }
    public string Resolution { get; set; }
    public ContentSourceType Type { get; set; }
}