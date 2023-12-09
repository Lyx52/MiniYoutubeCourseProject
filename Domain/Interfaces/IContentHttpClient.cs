using Domain.Model.Response;
using Domain.Model.View;

namespace Domain.Interfaces;

public interface IContentHttpClient
{
    Task<UploadVideoFileResponse> UploadVideoFile(UploadVideoModel model, CancellationToken cancellationToken = default(CancellationToken));
}