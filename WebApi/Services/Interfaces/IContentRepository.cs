using Domain.Entity;
using Domain.Model.Response;

namespace WebApi.Services.Interfaces;

public interface IContentRepository
{
    Task<CachedContentSource?> GetContentSource(Guid videoId, Guid sourceId,
        CancellationToken cancellationToken = default(CancellationToken));
}