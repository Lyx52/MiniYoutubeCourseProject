using Domain.Model.Response;

namespace Domain.Interfaces;

public interface IUserHttpClient
{
    Task<UserProfileResponse> GetUserProfile(CancellationToken cancellationToken = default(CancellationToken));
    Task<UserProfileResponse> GetPublicUserProfile(Guid userId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<CreatorProfileResponse> GetCreatorProfile(Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> Subscribe(string creatorId, CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> Unsubscribe(string creatorId, CancellationToken cancellationToken = default(CancellationToken));
    Task<UserNotificationResponse> GetUserNotifications(CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> DismissUserNotifications(string notificationId, CancellationToken cancellationToken = default(CancellationToken));
}