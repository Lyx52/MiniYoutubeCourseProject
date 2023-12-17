using Domain.Entity;

namespace WebApi.Services.Interfaces;

public interface INotificationRepository
{
    Task<Guid> AddNotification(string userId, string message, string link,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<UserNotification>> GetUserNotifications(string userId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task DismissNotification(string userId, string notificationId, CancellationToken cancellationToken = default(CancellationToken));
}