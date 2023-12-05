using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Request;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class VideoRepository : IVideoRepository
{
    private readonly ILogger<VideoRepository> _logger;
    private readonly VideoDbContext _dbContext;
    public VideoRepository(ILogger<VideoRepository> logger, VideoDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> CreateVideo(CreateVideoRequest payload, User creator, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        await _dbContext.Videos.AddAsync(new Video()
        {
            Id = id.ToString(),
            CreatorId = creator.Id,
            WorkSpaceId = payload.WorkSpaceId.ToString(),
            Title = payload.Title,
            Status = VideoProcessingStatus.CreatedMetadata,
            Description = payload.Description
        }, cancellationToken);
            
        await _dbContext.SaveChangesAsync(cancellationToken);    

        return id;
    }

    public async Task<bool> EnrichWithSources(Guid videoId, List<WorkFile> sources, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await GetVideoById(videoId, false, cancellationToken);
        if (video is null) return false;
        video.Sources = sources
            .Where((wf) => wf.Type != WorkFileType.Original)
            .Select((wf) => ConvertToContentSource(wf, video))
            .ToList();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateVideoStatus(Guid videoId, VideoProcessingStatus status, CancellationToken cancellationToken = default(CancellationToken))
    {
        _logger.LogInformation("Video {id} changed status to {status}", videoId, status);
        var video = await GetVideoById(videoId, false, cancellationToken);
        if (video is null) return false;
        video.Status = status;
        await _dbContext.SaveChangesAsync(cancellationToken);    
        
        return true;
    }

    public Task<Video?> GetVideoById(Guid id, bool includeSources = false, CancellationToken cancellationToken = default(CancellationToken))
    {
        return includeSources
            ? _dbContext.Videos
                .Include(v => v.Sources)
                .FirstOrDefaultAsync(v => v.Id.ToLower() == id.ToString().ToLower(), cancellationToken)
            : _dbContext.Videos
                .FirstOrDefaultAsync(v => v.Id.ToLower() == id.ToString().ToLower(), cancellationToken);
    }
    private ContentSource ConvertToContentSource(WorkFile file, Video video)
    {
        return new ContentSource()
        {
            Id = file.Id.ToString(),
            ContentType = file.Tags.FirstOrDefault((tag) => tag.StartsWith("ContentType"))?.Replace("ContentType=", string.Empty) ?? string.Empty,
            Video = video,
            Resolution = file.Tags.FirstOrDefault((tag) => tag.StartsWith("Resolution"))?.Replace("Resolution=", string.Empty) ?? string.Empty
        };
    }
}