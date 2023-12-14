using Domain.Constants;
using Domain.Entity;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface ICommentRepository
{
    Task<string> CreateComment(string userId, string videoId, string message, CancellationToken cancellationToken = default(CancellationToken));

    Task<IEnumerable<CommentModel>> GetByVideoIds(string videoId, string? userId = null,
        CancellationToken cancellationToken = default(CancellationToken));

    Task SetCommentImpression(string userId, string commentId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken));
}