using System.Text.Json.Serialization;
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
    public ContentSourceModel? Poster { get; set; } = null;
    public ContentSourceModel? PosterGif { get; set; } = null;
    public long ViewCount { get; set; }
    
    [JsonConstructor]
    public VideoPlaylistModel(string videoId, string title, string creatorId, string creatorName, string creatorIconLink, long viewCount)
    {
        VideoId = videoId;
        Title = title;
        CreatorId = creatorId;
        CreatorName = creatorName;
        CreatorIconLink = creatorIconLink;
        ViewCount = viewCount;
    }
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
        ViewCount = video.ViewCount;
    }
}