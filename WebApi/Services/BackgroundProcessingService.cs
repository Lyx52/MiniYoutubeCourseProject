using System.Collections.Concurrent;
using System.Runtime.Intrinsics.X86;
using System.Threading.Channels;
using Domain.Constants;
using WebApi.Services.Interfaces;
using WebApi.Services.Models;

namespace WebApi.Services;

public class BackgroundProcessingService : BackgroundService
{
    private readonly ILogger<BackgroundProcessingService> _logger;
    private readonly ChannelReader<BackgroundTask> _channel;
    private readonly ConcurrentBag<Task> _processingList;
    private readonly ConcurrentQueue<Task> _processingQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public BackgroundProcessingService(
        ILogger<BackgroundProcessingService> logger,
        ChannelReader<BackgroundTask> channel,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _channel = channel;
        _serviceScopeFactory = serviceScopeFactory;
        _processingList = new ConcurrentBag<Task>();
        _processingQueue = new ConcurrentQueue<Task>();
    }

    private async Task BuildTask(BackgroundTask task, CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        switch (task.Type)
        {
            case BackgroundTaskType.ProcessVideo:
            {
                var processingService = scope.ServiceProvider.GetRequiredService<IContentProcessingService>();
                if (task is not VideoTask payload) return;
                await processingService!.ProcessVideo(payload, cancellationToken);    
            } break;
            case BackgroundTaskType.PublishVideo:
            {
                var processingService = scope.ServiceProvider.GetRequiredService<IContentProcessingService>();
                if (task is not VideoTask payload) return;
                await processingService!.PublishVideo(payload, cancellationToken); 
            } break;
            case BackgroundTaskType.GenerateUploadNotifications:
            {
                var processingService = scope.ServiceProvider.GetRequiredService<INotificationProcessingService>();
                if (task is not NotificationTask payload) return;
                await processingService!.GenerateSubscriptionNotifications(payload, cancellationToken);          
            } break;
        }
        
    }

    private async Task ProcessTaskQueue(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Window in which jobs can be enqueued
            await Task.Delay(1000, cancellationToken);
            while(_processingQueue.TryDequeue(out var task))
                _processingList.Add(task);
            
            await Task.WhenAll(_processingList);
            _processingList.Clear();
        }
    }
    private async Task ProcessTaskChannel(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await foreach (var task in _channel.ReadAllAsync(cancellationToken))
            {
                _processingQueue.Enqueue(BuildTask(task, cancellationToken));
            }
        }
    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            ProcessTaskChannel(cancellationToken),
            ProcessTaskQueue(cancellationToken)
        );
    }
}