using Domain.Model.View;

namespace Domain.Model.Response;

public class VideoMetadataResponse : Response
{
    public string VideoId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public IEnumerable<ContentSourceModel> ContentSources { get; set; }
}