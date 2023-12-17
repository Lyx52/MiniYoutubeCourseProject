namespace Domain.Model.View;

public class VideoPlaylistModel
{
    public string VideoId { get; set; }
    public string Title { get; set; }
    public DateTime Created { get; set; }
    public string CreatorId { get; set; }
    public string CreatorName { get; set; }
    public string CreatorIconLink { get; set; }
    public ContentSourceModel? Poster { get; set; }
    public ContentSourceModel? PosterGif { get; set; }
}