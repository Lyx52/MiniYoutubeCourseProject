using Domain.Constants;
using Domain.Model.Response;

namespace Domain.Interfaces;

public interface ICommentHttpClient
{
    Task<QueryCommentsResponse> GetVideoComments(Guid videoId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> CreateComment(Guid videoId, string message,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> AddCommentImpression(Guid commentId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken));
}