using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Text;
using Domain.Constants;
using Domain.Model.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using WebApi.Services.Interfaces;
using static Domain.Constants.EmailTemplates;

namespace WebApi.Services;

public class ViewProcessingService : IViewProcessingService
{
    private readonly ILogger<ViewProcessingService> _logger;
    private readonly ApiConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private ConcurrentDictionary<Guid, long> ViewUpdates { get; set; } = new ConcurrentDictionary<Guid, long>();
    private const long MaxQueuedViewUpdates = 15;
    public ViewProcessingService(ILogger<ViewProcessingService> logger, 
        ApiConfiguration configuration, 
        IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task IncrementVideoViewCount(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var queuedViews = ViewUpdates.AddOrUpdate(videoId, 1, (id, count) => count + 1);
        if (queuedViews < MaxQueuedViewUpdates + Random.Shared.Next(-5, 5)) return; // Introduce some randomness in view count
        if (ViewUpdates.TryRemove(videoId, out var views))
        {
            await using (var scope = _serviceScopeFactory.CreateAsyncScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<IVideoRepository>()
                    .UpdateVideoViewCount(videoId, views, cancellationToken);    
            }
        }
    }
}