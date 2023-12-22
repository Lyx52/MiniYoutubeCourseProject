using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Response;
using Domain.Model.View;

namespace Domain.Interfaces;

public interface IVideoHttpClient
{
    Task<CreateVideoResponse> CreateVideo(CreateVideoModel model,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> PublishVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoStatusResponse> GetProcessingStatus(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<SearchVideosResponse> GetVideosByTitle(string searchText, CancellationToken requestCancellationToken = default(CancellationToken));
    Task<VideoMetadataResponse> GetVideoMetadata(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoPlaylistResponse> GetVideoPlaylist(VideoQuery query, CancellationToken cancellationToken = default(CancellationToken));
    Task<UserVideosResponse> GetUserVideos(int page, int pageSize, CancellationToken cancellationToken = default(CancellationToken));
    Task AddVideoImpression(string videoId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken));
}