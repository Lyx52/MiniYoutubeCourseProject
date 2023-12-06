using Domain.Constants;

namespace Domain.Model.Response;

public class CachedContentSource
{
    public string SourceId { get; set; }
    public string VideoId { get; set; }
    public string FileLocation { get; set; }
    public byte[] Data { get; set; }
    public string ContentType { get; set; }
    public ContentSourceType Type { get; set; }
}