using Domain.Entity;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface ISubscriberRepository
{
    Task<IEnumerable<Subscriber>> GetSubscribers(Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<UserModel>> GetSubscribedCreators(Guid userId,
        CancellationToken cancellationToken = default(CancellationToken));

    Task Subscribe(Guid userId, Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task Unsubscribe(Guid userId, Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> IsSubscribed(Guid userId, Guid creatorId, CancellationToken cancellationToken = default(CancellationToken));
    Task<long> GetSubscriberCount(Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
}