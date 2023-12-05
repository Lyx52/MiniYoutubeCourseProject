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
    private readonly ChannelReader<ProcessVideoTask> _channel;
    private readonly ConcurrentBag<Task> _processingList;
    private readonly ConcurrentQueue<Task> _processingQueue;
    private readonly IContentProcessingService _processingService;
    public BackgroundProcessingService(
        ILogger<BackgroundProcessingService> logger,
        ChannelReader<ProcessVideoTask> channel,
        IContentProcessingService processingService)
    {
        _logger = logger;
        _channel = channel;
        _processingService = processingService;
        _processingList = new ConcurrentBag<Task>();
        _processingQueue = new ConcurrentQueue<Task>();
    }

    private async Task BuildTask(ProcessVideoTask payload)
    {
        await Task.Delay(3000);
        
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
                _processingQueue.Enqueue(BuildTask(task));
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