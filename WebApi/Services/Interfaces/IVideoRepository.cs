using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface IVideoRepository
{
    Task<bool> UpdateVideoStatus(Guid videoId, VideoProcessingStatus status, CancellationToken cancellationToken = default(CancellationToken));
    Task<Guid> CreateVideo(CreateVideoRequest payload, string creatorId, CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> EnrichWithSources(Guid videoId, List<WorkFile> sources, CancellationToken cancellationToken = default(CancellationToken));
    Task<Video?> GetVideoById(Guid id, bool includeSourcesImpressions = false, CancellationToken cancellationToken = default(CancellationToken));
    Task<Video?> GetVideoById(Guid id, string userId, CancellationToken cancellationToken = default(CancellationToken));
    Task<List<ContentSource>> GetVideoSourcesById(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoProcessingStatus?> GetVideoStatus(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<List<Video>> QueryVideosByTitle(string searchText, CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<VideoPlaylistModel>> GetVideoPlaylist(VideoQuery query, CancellationToken cancellationToken = default(CancellationToken));
    Task SetVideoImpression(string userId, string videoId, ImpressionType impressionType, CancellationToken cancellationToken = default(CancellationToken));
}