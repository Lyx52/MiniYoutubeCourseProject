using Microsoft.AspNetCore.WebUtilities;
using WebApi.Services.Models;

namespace WebApi.Services.Interfaces;

public interface IContentService
{
    Task<SaveTemporaryFileResult> SaveTemporaryFile(MemoryStream readStream, string fileName, CancellationToken cancellationToken = default(CancellationToken));
}