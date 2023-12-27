﻿using Domain.Entity;

namespace WebApi.Services.Interfaces;

public interface IPlaylistRepository
{
    Task<Guid> CreatePlaylist(Guid userId, string title, CancellationToken cancellationToken = default(CancellationToken));
    Task<Guid> AddVideoToPlaylist(Guid userId, Guid videoId, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task RemoveVideoFromPlaylist(Guid userId, Guid videoId, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Guid[]> AddVideosToPlaylist(Guid userId, IEnumerable<Guid> videoIds, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task RemoveVideosFromPlaylist(Guid userId, IEnumerable<Guid> videoIds, Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<Video>> GetPlaylistVideos(Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<List<Playlist>> GetPlaylists(Guid? userId = null,
        CancellationToken cancellationToken = default(CancellationToken));
}