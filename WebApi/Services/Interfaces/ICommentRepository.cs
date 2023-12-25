using Domain.Constants;
using Domain.Entity;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface ICommentRepository
{
    Task<string> CreateComment(Guid userId, Guid videoId, string message, CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<CommentModel>> GetByVideoIds(Guid videoId, Guid? userId = null, CancellationToken cancellationToken = default(CancellationToken));
    Task SetCommentImpression(Guid userId, Guid commentId, ImpressionType impressionType, CancellationToken cancellationToken = default(CancellationToken));
}