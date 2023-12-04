using Microsoft.AspNetCore.WebUtilities;

namespace WebApi.Services.Interfaces;

public interface IContentService
{
    Task<Guid?> SaveTemporaryFile(MemoryStream readStream, string fileName, CancellationToken cancellationToken = default(CancellationToken));
}