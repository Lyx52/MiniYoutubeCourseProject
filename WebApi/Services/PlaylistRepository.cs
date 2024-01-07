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

    public async Task UpdatePlaylist(Guid userId, Guid playlistId, string title, IEnumerable<Guid> videoIds,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var playlistVideos = await _dbContext.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId.ToString())
            .ToListAsync(cancellationToken);
        var remainList = playlistVideos
            .Where(pv => videoIds.Contains(Guid.Parse(pv.VideoId)))
            .Select(pv => Guid.Parse(pv.VideoId));
        var deleteList = playlistVideos.Where(pv => !remainList.Contains(Guid.Parse(pv.VideoId)))
            .Select(pv => Guid.Parse(pv.VideoId));
        var addList = videoIds.Where((id) => !remainList.Contains(id) && !deleteList.Contains(id));

        await RemoveVideosFromPlaylist(userId, deleteList, playlistId, cancellationToken);
        await AddVideosToPlaylist(userId, addList, playlistId, cancellationToken);
    }

    public async Task<Guid> AddVideoToPlaylist(Guid userId, Guid videoId, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var entity = await _dbContext.PlaylistVideos
            .FirstOrDefaultAsync(pv => pv.VideoId == videoId.ToString() && pv.PlaylistId == playlistId.ToString(), cancellationToken);
        if (entity is not null) return Guid.Parse(entity.Id);
        
        var video = await _dbContext.Videos
            .FirstOrDefaultAsync(v => v.CreatorId == userId.ToString() && v.Id == videoId.ToString(), cancellationToken);
        if (video is null) return Guid.Empty;
        
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(v => v.CreatorId == userId.ToString() && v.Id == playlistId.ToString(), cancellationToken);
        if (playlist is null) return Guid.Empty;
        
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
        var entity = await _dbContext.PlaylistVideos
            .Include(pv => pv.Playlist)
            .FirstOrDefaultAsync(pv => 
                pv.VideoId == videoId.ToString() && 
                pv.PlaylistId == playlistId.ToString() &&
                pv.Playlist.CreatorId == userId.ToString(), cancellationToken);
        if (entity is null) return;
        _dbContext.PlaylistVideos.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Guid>> AddVideosToPlaylist(Guid userId, IEnumerable<Guid> videoIds, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var playlistVideos = new List<Guid>();
        foreach (var videoId in videoIds)
        {
            playlistVideos.Add(await AddVideoToPlaylist(userId, videoId, playlistId, cancellationToken));
        }

        return playlistVideos;
    }

    public async Task RemoveVideosFromPlaylist(Guid userId, IEnumerable<Guid> videoIds, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        foreach (var videoId in videoIds)
        {
            await RemoveVideoFromPlaylist(userId, videoId, playlistId, cancellationToken);
        }
    }

    public Task<List<Video>> GetPlaylistVideos(Guid playlistId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.PlaylistVideos
            .Include(pv => pv.Playlist)
            .Include(pv => pv.Video)
            .Where(pv => pv.PlaylistId == playlistId.ToString())
            .Select(pv => pv.Video)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Playlist>> GetPlaylists(Guid? userId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Playlists
            .Where(p => userId.HasValue || p.CreatorId == userId.ToString())
            .ToListAsync(cancellationToken);
    }

    public Task<List<PlaylistModel>> GetPlaylistModels(Guid? userId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.PlaylistVideos
            .Include(pv => pv.Playlist)
            .Include(pv => pv.Video)
            .Where(pv => userId.HasValue && pv.Playlist.CreatorId == userId.ToString())
            .GroupBy(pv => pv.Playlist)
            .Select(pg => new PlaylistModel()
            {
                Title = pg.Key.Title,
                PlaylistId = Guid.Parse(pg.Key.Id),
                CreatorId = Guid.Parse(pg.Key.CreatorId),
                Videos = pg.Select(pv => Guid.Parse(pv.VideoId)).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task DeletePlaylist(Guid userId, Guid playlistId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var playlist = await _dbContext.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId.ToString() && p.CreatorId == userId.ToString(), cancellationToken);
        if (playlist is null) return;
        _dbContext.Playlists.Remove(playlist);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Playlist?> GetPlaylistById(Guid playlistId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Playlists
            .FirstOrDefaultAsync(cancellationToken);
    }
}