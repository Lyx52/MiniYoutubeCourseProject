using System.Net;
using System.Net.Http.Json;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class UserHttpClient(
    ILogger<UserHttpClient> logger,
    IHttpClientFactory httpClientFactory,
    ILoginManager loginManager)
    : BaseHttpClient(nameof(UserHttpClient), logger, httpClientFactory, loginManager), IUserHttpClient
{
    private readonly ILoginManager _loginManager = loginManager;

    public Task<UserProfileResponse> GetPublicUserProfile(Guid userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var qb = new QueryBuilder { { "userId", userId.ToString() } };
        return SendQueryRequest<UserProfileResponse>(HttpMethod.Get, "api/User/PublicProfile", qb.ToQueryString(), JwtRequirement.None, cancellationToken);
    }

    public Task<CreatorProfileResponse> GetCreatorProfile(Guid creatorId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var qb = new QueryBuilder { { "creatorId", creatorId.ToString() } };
        return SendQueryRequest<CreatorProfileResponse>(HttpMethod.Get, "api/User/CreatorProfile", qb.ToQueryString(), JwtRequirement.Optional, cancellationToken);
    }

    public Task<Response> UpdateUserProfile(UserModel userModel, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<UpdateUserProfileRequest, Response>("api/User/UpdateProfile", new UpdateUserProfileRequest()
        {
            CreatorName = userModel.CreatorName,
            IconLink = userModel.IconLink
        }, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<UserProfileResponse> GetUserProfile(CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendQueryRequest<UserProfileResponse>(HttpMethod.Get, "api/User/Profile", QueryString.Empty, JwtRequirement.Mandatory, cancellationToken);
    }
    
    public Task<Response> Subscribe(Guid creatorId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<SubscribeRequest, Response>("api/User/Subscribe", new SubscribeRequest()
        {
            CreatorId = creatorId
        }, JwtRequirement.Mandatory, cancellationToken);
    }
    
    public Task<Response> Unsubscribe(Guid creatorId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<UnsubscribeRequest, Response>("api/User/Unsubscribe", new UnsubscribeRequest()
        {
            CreatorId = creatorId
        }, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<UserNotificationResponse> GetUserNotifications(CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendQueryRequest<UserNotificationResponse>(HttpMethod.Get, "api/User/Notifications", 
            QueryString.Empty, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<Response> DismissUserNotifications(Guid notificationId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<DismissNotificationRequest, Response>("api/User/DismissNotification", new DismissNotificationRequest()
        {
            NotificationId = notificationId
        }, JwtRequirement.Mandatory, cancellationToken);
    }
}