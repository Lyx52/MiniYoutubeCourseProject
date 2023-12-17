using Domain.Constants;
using Domain.Entity;
using Domain.Model;
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

    public async Task<Guid> CreateVideo(CreateVideoRequest payload, string creatorId, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        await _dbContext.Videos.AddAsync(new Video()
        {
            Id = id.ToString(),
            CreatorId = creatorId,
            WorkSpaceId = payload.WorkSpaceId.ToString(),
            Title = payload.Title,
            Status = VideoProcessingStatus.CreatedMetadata,
            Description = payload.Description,
            IsUnlisted = payload.IsUnlisted,
            Created = DateTime.UtcNow
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

    public Task<Video?> GetVideoById(Guid id, bool includeSourcesImpressions = false, CancellationToken cancellationToken = default(CancellationToken))
    {
        return includeSourcesImpressions
            ? _dbContext.Videos
                .Include(v => v.Sources)
                .Include(v => v.Impressions)
                .FirstOrDefaultAsync(v => v.Id.ToLower() == id.ToString().ToLower(), cancellationToken)
            : _dbContext.Videos
                .FirstOrDefaultAsync(v => v.Id.ToLower() == id.ToString().ToLower(), cancellationToken);
    }
    public Task<Video?> GetVideoById(Guid id, string userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Videos.FirstOrDefaultAsync((v) =>
            v.CreatorId.ToLower() == userId.ToLower() &&
            v.Id.ToLower() == id.ToString().ToLower(), cancellationToken);
    }

    public Task<List<ContentSource>> GetVideoSourcesById(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Sources
            .Where((s) => s.VideoId.ToLower() == videoId.ToString().ToLower())
            .ToListAsync(cancellationToken);
    }

    public async Task<VideoProcessingStatus?> GetVideoStatus(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await GetVideoById(videoId, false, cancellationToken);
        return video?.Status;
    }

    public Task<List<Video>> QueryVideosByTitle(string searchText, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Videos
            .Where((v) => v.Title.ToLower().Contains(searchText.ToLower()))
            .OrderBy((v) => v.Created)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VideoPlaylistModel>> GetVideoPlaylist(VideoQuery query, CancellationToken cancellationToken = default(CancellationToken))
    {
        var queryable = _dbContext.Videos
            .Include((v) => v.Sources).AsQueryable();
        
        if (query.OrderByCreated) 
            queryable = queryable.OrderBy((v) => v.Created);

        if (query.CreatorId != Guid.Empty) 
            queryable = queryable.Where(v => v.CreatorId == query.CreatorId.ToString());
        
        var videos = await 
            queryable
            .Where(v => v.IsUnlisted == query.IncludeUnlisted)
            .Skip(query.From)
            .Take(query.Count)
            .ToListAsync(cancellationToken);
        var creators = await _userRepository.GetUsersByIds(videos.Select(v => v.CreatorId), cancellationToken);
        return videos
            .Select((v) =>
            {
                var poster = v.Sources!.First((s) => s.Type == ContentSourceType.Thumbnail);
                var posterGif = v.Sources!.First((s) => s.Type == ContentSourceType.ThumbnailGif);
                var creator = creators.FirstOrDefault(c => c.Id == v.CreatorId);
                if (creator is null) return null;
                return new VideoPlaylistModel()
                {
                    Created = v.Created,
                    Title = v.Title,
                    VideoId = v.Id,
                    CreatorId = creator.Id,
                    CreatorName = creator.CreatorName,
                    CreatorIconLink = creator.IconLink,
                    Poster = new ContentSourceModel()
                    {
                        ContentType = poster.ContentType,
                        Id = poster.Id,
                        Resolution = poster.Resolution,
                        Type = poster.Type
                    },
                    PosterGif = new ContentSourceModel()
                    {
                        ContentType = posterGif.ContentType,
                        Id = posterGif.Id,
                        Resolution = posterGif.Resolution,
                        Type = posterGif.Type
                    },
                };
            })
            .Where(m => m is not null)!;
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
    
    public async Task SetVideoImpression(string userId, string videoId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var compositeId = $"{userId[0..18]}-{videoId[19..36]}";
        var impression =
            await _dbContext.VideoImpressions.FirstOrDefaultAsync(
                ci => ci.Id == compositeId, cancellationToken);
        if (impression is not null)
        {
            impression.Impression = impressionType;
        }
        else
        {
            var video = await _dbContext.Videos.FirstOrDefaultAsync(c => c.Id == videoId, cancellationToken);
            if (video is null) return;
            await _dbContext.VideoImpressions.AddAsync(new VideoImpression()
            {
                Id = compositeId,
                Impression = impressionType,
                VideoId = videoId,
                Video = video
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}