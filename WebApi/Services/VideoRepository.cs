using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Query;
using Domain.Model.Request;
using Domain.Model.View;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class VideoRepository : IVideoRepository
{
    private readonly ILogger<VideoRepository> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    public VideoRepository(ILogger<VideoRepository> logger, ApplicationDbContext dbContext, IUserRepository userRepository)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userRepository = userRepository;
    }
    public async Task<Guid> CreateVideo(CreateVideoRequest payload, Guid creatorId, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        await _dbContext.Videos.AddAsync(new Video()
        {
            Id = id.ToString(),
            CreatorId = creatorId.ToString(),
            WorkSpaceId = payload.WorkSpaceId.ToString(),
            Title = payload.Title,
            Status = VideoProcessingStatus.CreatedMetadata,
            Description = payload.Description,
            IsUnlisted = payload.IsUnlisted,
            Created = DateTime.UtcNow,
            NotificationsSent = false
        }, cancellationToken);
            
        await _dbContext.SaveChangesAsync(cancellationToken);
        return id;
    }

    public Task<List<ContentSource>> GetVideoSourcesById(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Sources
            .Where(s => s.VideoId == videoId.ToString())
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EnrichWithSources(Guid videoId, List<WorkFile> sources, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken);
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
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken);
        if (video is null) return false;
        video.Status = status;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private ContentSource ConvertToContentSource(WorkFile file, Video video)
    {
        return new ContentSource()
        {
            Id = file.Id.ToString(),
            Type = file.Type switch
            {
                WorkFileType.Poster => ContentSourceType.Thumbnail,
                WorkFileType.EncodedSource => ContentSourceType.Video,
                WorkFileType.PosterGif => ContentSourceType.ThumbnailGif,
                _ => ContentSourceType.Video
            },
            ContentType = file.Tags.FirstOrDefault((tag) => tag.StartsWith("ContentType"))?.Replace("ContentType=", string.Empty) ?? string.Empty,
            Video = video,
            Resolution = file.Tags.FirstOrDefault((tag) => tag.StartsWith("Resolution"))?.Replace("Resolution=", string.Empty) ?? string.Empty
        };
    }
    
    public async Task SetVideoImpression(Guid userId, Guid videoId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var compositeId = $"{userId.ToString()[0..18]}-{videoId.ToString()[19..36]}";
        var impression =
            await _dbContext.VideoImpressions.FirstOrDefaultAsync(
                ci => ci.Id == compositeId, cancellationToken);
        if (impression is not null)
        {
            impression.Impression = impressionType;
        }
        else
        {
            var video = await _dbContext.Videos.FirstOrDefaultAsync(c => c.Id == videoId.ToString(), cancellationToken);
            if (video is null) return;
            await _dbContext.VideoImpressions.AddAsync(new VideoImpression()
            {
                Id = compositeId,
                Impression = impressionType,
                VideoId = videoId.ToString(),
                Video = video
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Video?> GetVideoById(Guid videoId, bool includeAll = false,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return
            includeAll ? 
            _dbContext.Videos
                .Include(v => v.Sources)
                .Include(v => v.Impressions)
                .Include(v => v.Comments)
                .FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken)
            : 
            _dbContext.Videos
                .FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken);
    }

    public async Task DeleteVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext
            .Videos
            .Include(v => v.Impressions)
            .Include(v => v.Comments)
            .Include(v => v.Sources)
            .FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken);
        if (video is null) return;
        _dbContext.Videos.Remove(video);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> UpdateVideo(Video video,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        await _dbContext.SaveChangesAsync(cancellationToken);    

        return Guid.Parse(video.Id);
    }

    public async Task ChangeVisibility(Guid videoId, bool isUnlisted, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await GetVideoById(videoId, false, cancellationToken);
        if (video is null) return;
        video.IsUnlisted = isUnlisted;
        await _dbContext.SaveChangesAsync(cancellationToken);
        var x = await _dbContext.Videos.ToListAsync(cancellationToken);
    }

    public async Task<VideoMetadataModel?> GetVideoMetadataById(Guid videoId, Guid? userId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos
            .Include(v => v.Impressions)
            .Include(v => v.Sources)
            .FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken);
        if (video is null) return null;
         
        var dislikes = video.Impressions
            .LongCount(i => i.Impression == ImpressionType.Dislike);
        var likes = video.Impressions
            .LongCount(i => i.Impression == ImpressionType.Like);

        var userImpression = ImpressionType.None;
        if (userId.HasValue)
        {
            var compositeId = $"{userId.Value.ToString()[0..18]}-{video.Id.ToString()[19..36]}";
            userImpression = video.Impressions
                .FirstOrDefault(i => i.Id == compositeId)?.Impression ?? ImpressionType.None;
        }

        var contentSources = video.Sources?.Select((s) => new ContentSourceModel(s)) ?? new List<ContentSourceModel>();
        
        return new VideoMetadataModel()
        {
            Description = video.Description,
            Title = video.Title,
            VideoId = videoId,
            CreatorId = Guid.Parse(video.CreatorId),
            Dislikes = dislikes,
            Likes = likes,
            UserImpression = userImpression,
            ContentSources = contentSources
        };
    }

    public Task<List<Video>> QueryAsync(IEntityQuery<Video> query, bool asNonTracking = true, CancellationToken cancellationToken = default(CancellationToken)) =>
        query
            .ToQueryable(_dbContext.Videos, asNonTracking)
            .ToListAsync(cancellationToken);

    public Task<int> QueryCountAsync(IEntityQuery<Video> query, CancellationToken cancellationToken = default(CancellationToken))
    {
        return query
            .ToQueryable(_dbContext.Videos, true)
            .CountAsync(cancellationToken);
    }
}