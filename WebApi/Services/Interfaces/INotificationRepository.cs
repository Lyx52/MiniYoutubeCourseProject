using Domain.Entity;

namespace WebApi.Services.Interfaces;

public interface INotificationRepository
{
    Task<Guid> AddNotification(Guid userId, string message, string link,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<UserNotification>> GetUserNotifications(Guid userId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task DismissNotification(Guid userId, Guid notificationId, CancellationToken cancellationToken = default(CancellationToken));
}