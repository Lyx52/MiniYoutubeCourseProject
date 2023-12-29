using System.Security.Claims;
using Domain.Constants;
using Domain.Entity;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly ILogger<PlaylistRepository> _logger;
    private readonly ApplicationDbContext _dbContext;
    public PlaylistRepository(ILogger<PlaylistRepository> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Guid> CreatePlaylist(Guid userId, string title, CancellationToken cancellationToken = default(CancellationToken))
    {
        var id = Guid.NewGuid();
        await _dbContext.Playlists.AddAsync(new Playlist()
        {
            Id = id.ToString(),
            Title = title,
            CreatorId = userId.ToString()
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return id;
    }
    
    public async Task<Guid> AddVideoToPlaylist(Guid userId, Guid videoId, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.CreatorId == userId.ToString() && v.Id == videoId.ToString(), cancellationToken);
        if (video is null) return Guid.Empty;
        
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(v => v.CreatorId == userId.ToString() && v.Id == playlistId.ToString(), cancellationToken);
        if (playlist is null) return Guid.Empty;
        
        var entity = await _dbContext.PlaylistVideos
            .FirstOrDefaultAsync(pv => pv.VideoId == videoId.ToString() && pv.PlaylistId == playlistId.ToString(), cancellationToken);
        if (entity is not null) return Guid.Parse(entity.Id);
        var id = Guid.NewGuid();

        await _dbContext.PlaylistVideos.AddAsync(new PlaylistVideo()
        {
            Id = id.ToString(),
            Video = video,
            Playlist = playlist
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task RemoveVideoFromPlaylist(Guid userId, Guid videoId, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(v => v.CreatorId == userId.ToString() && v.Id == playlistId.ToString(), cancellationToken);
        if (playlist is null) return;
        
        var entity = await _dbContext.PlaylistVideos
            .FirstOrDefaultAsync(pv => 
                pv.VideoId == videoId.ToString() && 
                pv.PlaylistId == playlist.Id, cancellationToken);
        if (entity is null) return;
        _dbContext.PlaylistVideos.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Guid[]> AddVideosToPlaylist(Guid userId, IEnumerable<Guid> videoIds, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var tasks = videoIds.Select(vid => AddVideoToPlaylist(userId, vid, playlistId, cancellationToken));
        return Task.WhenAll(tasks);
    }

    public Task RemoveVideosFromPlaylist(Guid userId, IEnumerable<Guid> videoIds, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var tasks = videoIds.Select(vid => RemoveVideoFromPlaylist(userId, vid, playlistId, cancellationToken));
        return Task.WhenAll(tasks);
    }

    public async Task<IEnumerable<Video>> GetPlaylistVideos(Guid playlistId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var playlist = await _dbContext.Playlists
            .Include(p => p.Videos)
            .FirstOrDefaultAsync(p => p.Id == playlistId.ToString(), cancellationToken);
        return playlist is null ? new List<Video>() : playlist.Videos.ToList();
    }

    public Task<List<Playlist>> GetPlaylists(Guid? userId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Playlists
            .Where(p => userId.HasValue || p.CreatorId == userId.ToString())
            .ToListAsync(cancellationToken);
    }

    public Task<List<PlaylistModel>> GetPlaylistModels(Guid? userId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Playlists
            .Include(p => p.Videos)
            .Where(p => userId.HasValue || p.CreatorId == userId.ToString())
            .Select(p => new PlaylistModel()
            {
                Title = p.Title,
                CreatorId = Guid.Parse(p.CreatorId),
                PlaylistId = Guid.Parse(p.Id),
                Videos = p.Videos.Select(v => Guid.Parse(v.Id))
            })
            .ToListAsync(cancellationToken);
    }

    public Task<Playlist?> GetPlaylistById(Guid playlistId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Playlists
            .FirstOrDefaultAsync(cancellationToken);
    }
}