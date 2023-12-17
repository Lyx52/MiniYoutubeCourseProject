using WebApi.Services.Models;

namespace WebApi.Services.Interfaces;

public interface INotificationProcessingService
{
    Task GenerateSubscriptionNotifications(NotificationTask payload,
        CancellationToken cancellationToken = default(CancellationToken));
}