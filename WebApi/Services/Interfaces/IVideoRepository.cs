using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Query;
using Domain.Model.Request;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface IVideoRepository : IEntityRepository<Video, VideoModel, string>
{
    Task<Guid> CreateVideo(CreateVideoRequest payload, Guid creatorId, CancellationToken cancellationToken);
    Task<bool> UpdateVideoStatus(Guid videoId, VideoProcessingStatus status, CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> EnrichWithSources(Guid videoId, List<WorkFile> sources, CancellationToken cancellationToken = default(CancellationToken));
    Task<List<ContentSource>> GetVideoSourcesById(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task SetVideoImpression(Guid userId, Guid videoId, ImpressionType impressionType, CancellationToken cancellationToken = default(CancellationToken));
    Task<Video?> GetVideoById(Guid videoId, bool includeAll = false, CancellationToken cancellationToken = default(CancellationToken));
    Task DeleteVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
    Task<Guid> UpdateVideo(Video video, CancellationToken cancellationToken = default(CancellationToken));
    Task ChangeVisibility(Guid videoId, bool isUnlisted, CancellationToken cancellationToken = default(CancellationToken));
    Task<VideoMetadataModel?> GetVideoMetadataById(Guid videoId, Guid? userId = null,
        CancellationToken cancellationToken = default(CancellationToken));
}