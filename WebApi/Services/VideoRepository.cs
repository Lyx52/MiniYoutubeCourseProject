using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class VideoRepository : IVideoRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<VideoRepository> _logger;
    public VideoRepository(ILogger<VideoRepository> logger, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> UpdateVideoStatus(Guid videoId, VideoProcessingStatus status, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id.ToLower() == videoId.ToString().ToLower(), cancellationToken);
        if (video is null) return false;
        video.Status = status;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}