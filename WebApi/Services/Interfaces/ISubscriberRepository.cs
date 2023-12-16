using Domain.Entity;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface ISubscriberRepository
{
    Task<IEnumerable<Subscriber>> GetSubscribers(string creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<UserModel>> GetSubscribedCreators(string userId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Guid> Subscribe(string userId, string creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task Unsubscribe(string userId, string creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
}