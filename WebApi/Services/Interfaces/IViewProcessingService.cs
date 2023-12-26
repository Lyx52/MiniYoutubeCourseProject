namespace WebApi.Services.Interfaces;

public interface IViewProcessingService
{
    Task IncrementVideoViewCount(Guid videoId, CancellationToken cancellationToken = default(CancellationToken));
}