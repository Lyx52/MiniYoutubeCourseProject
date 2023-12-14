using Domain.Constants;
using Domain.Model.Response;

namespace Domain.Interfaces;

public interface ICommentHttpClient
{
    Task<QueryCommentsResponse> GetVideoComments(string videoId,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<Response> CreateComment(Guid videoId, string message,
        CancellationToken cancellationToken = default(CancellationToken));

    Task AddCommentImpression(string commentId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken));
}