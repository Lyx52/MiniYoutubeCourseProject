using Domain.Entity;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface ICommentRepository
{
    Task<string> CreateComment(string userId, string videoId, string message, CancellationToken cancellationToken = default(CancellationToken));

    Task<IEnumerable<CommentModel>> GetByVideoIds(string videoId,
        CancellationToken cancellationToken = default(CancellationToken));

    Task LikeDislikeComment(string payloadCommentId, bool payloadIsLike, CancellationToken cancellationToken = default(CancellationToken));
}