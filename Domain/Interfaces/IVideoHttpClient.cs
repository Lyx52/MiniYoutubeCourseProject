using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Response;

namespace Domain.Interfaces;

public interface IVideoHttpClient
{
    Task<CreateVideoResponse> CreateVideo(CreateVideoModel model,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<Response> PublishVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoStatusResponse> GetProcessingStatus(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<SearchVideosResponse> GetVideosByTitle(string searchText, CancellationToken requestCancellationToken = default(CancellationToken));
    Task<VideoMetadataResponse> GetVideoMetadata(Guid videoId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoPlaylistResponse> GetVideoPlaylist(int from, int count,
        CancellationToken cancellationToken = default(CancellationToken));
}