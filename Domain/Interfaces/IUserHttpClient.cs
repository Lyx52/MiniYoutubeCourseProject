using Domain.Model.Response;
using Domain.Model.View;

namespace Domain.Interfaces;

public interface IUserHttpClient
{
    Task<Response> UpdateUserProfile(UserModel userModel, CancellationToken cancellationToken = default(CancellationToken));
    Task<UserProfileResponse> GetUserProfile(CancellationToken cancellationToken = default(CancellationToken));
    Task<UserProfileResponse> GetPublicUserProfile(Guid userId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<CreatorProfileResponse> GetCreatorProfile(Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> Subscribe(Guid creatorId, CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> Unsubscribe(Guid creatorId, CancellationToken cancellationToken = default(CancellationToken));
    Task<UserNotificationResponse> GetUserNotifications(CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> DismissUserNotifications(Guid notificationId, CancellationToken cancellationToken = default(CancellationToken));
}