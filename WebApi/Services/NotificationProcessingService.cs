using Domain.Constants;
using Domain.Model.Query;
using WebApi.Services.Interfaces;
using WebApi.Services.Models;

namespace WebApi.Services;

public class NotificationProcessingService : INotificationProcessingService
{
    private readonly ILogger<NotificationProcessingService> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    public NotificationProcessingService(ILogger<NotificationProcessingService> logger,
        IVideoRepository videoRepository,
        ISubscriberRepository subscriberRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository)
    {
        _videoRepository = videoRepository;
        _subscriberRepository = subscriberRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }
    public async Task GenerateSubscriptionNotifications(NotificationTask payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _videoRepository.GetVideoById(payload.VideoId, false, cancellationToken);
        if (video is null)
        {
            _logger.LogWarning("Failed to generate notifications {VideoId} video does not exist", payload.VideoId);
            return;
        }

        if (video.IsUnlisted)
        {
            _logger.LogInformation("Video {VideoId} is unlisted, won't generate notifications", payload.VideoId);
            return;
        }

        if (video.NotificationsSent) return;
        var subscribers = await _subscriberRepository
            .GetSubscribers(Guid.Parse(video.CreatorId), cancellationToken);
        var message = $"New video '{video.Title}'";
        var redirectLink = $"/watch/{Guid.Parse(video.Id).ToEncodedId()}";
        
        var tasks = subscribers.Select(s =>
            _notificationRepository
                .AddNotification(Guid.Parse(s.SubscriberId), message, redirectLink, cancellationToken)
        ).ToArray();
        
        _logger.LogInformation("Processing {NotificationTaskCount} notifications", tasks.Length);
        video.NotificationsSent = true;
        await _videoRepository.UpdateVideo(video, cancellationToken);
        await Task.WhenAll(tasks);
    }
}