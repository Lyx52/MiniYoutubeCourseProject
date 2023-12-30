using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Query;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;

namespace Domain.Interfaces;

public interface IVideoHttpClient
{
    Task<CreateOrUpdateVideoResponse> CreateVideo(EditVideoMetadataModel model,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> PublishVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoStatusResponse> GetProcessingStatus(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<QueryVideosResponse> GetVideosByTitle(string searchText, int from, int count,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoMetadataResponse> GetVideoMetadata(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoPlaylistResponse> GetVideoPlaylist(GetVideoPlaylistModel model, CancellationToken cancellationToken = default(CancellationToken));
    Task<QueryVideosResponse> GetUserVideos(int from, int count, CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> AddVideoImpression(Guid videoId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> ChangeVideoVisibility(Guid videoId, bool isPrivate,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> DeleteVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<CreateOrUpdateVideoResponse> UpdateVideo(EditVideoMetadataModel model,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<CreateOrUpdatePlaylistResponse> CreatePlaylist(EditPlaylistModel model,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<CreateOrUpdatePlaylistResponse> UpdatePlaylist(EditPlaylistModel model,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<CreatorPlaylistsResponse> GetCreatorPlaylists(Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> DeletePlaylist(Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> AddVideosToPlaylist(IEnumerable<Guid> videos, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> RemoveVideosFromPlaylist(IEnumerable<Guid> videos, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<CreatorPlaylistsResponse> GetUserPlaylists(CancellationToken cancellationToken = default(CancellationToken));
}