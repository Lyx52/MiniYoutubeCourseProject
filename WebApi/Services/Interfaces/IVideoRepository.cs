using Domain.Constants;

namespace WebApi.Services.Interfaces;

public interface IVideoRepository
{
    Task<bool> UpdateVideoStatus(Guid videoId, VideoProcessingStatus status, CancellationToken cancellationToken = default(CancellationToken));
}