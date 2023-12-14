using Domain.Model.Response;

namespace Domain.Interfaces;

public interface ICommentHttpClient
{
    Task<QueryCommentsResponse> GetVideoComments(string videoId,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<Response> CreateComment(Guid videoId, string message,
        CancellationToken cancellationToken = default(CancellationToken));

    Task AddLikeDislike(string commentId, bool isLike,
        CancellationToken cancellationToken = default(CancellationToken));
}