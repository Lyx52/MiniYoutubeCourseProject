using Domain.Constants;
using Domain.Entity;

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

    public VideoPlaylistModel(Video video, UserModel creator)
    {
        if (video.Sources?.FirstOrDefault((s) => s.Type == ContentSourceType.Thumbnail) is {} poster)
        {
            Poster = new ContentSourceModel(poster);
        }
        if (video.Sources?.FirstOrDefault((s) => s.Type == ContentSourceType.ThumbnailGif) is {} posterGif)
        {
            PosterGif = new ContentSourceModel(posterGif);
        }

        Created = video.Created;
        Title = video.Title;
        VideoId = video.Id;
        CreatorId = creator.Id;
        CreatorName = creator.CreatorName;
        CreatorIconLink = creator.IconLink;
    }
}