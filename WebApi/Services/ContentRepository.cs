﻿using Domain.Constants;
using Domain.Model.Response;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class ContentRepository : IContentRepository
{
    private readonly ILogger<ContentRepository> _logger;
    private readonly VideoDbContext _dbContext;
    private readonly IWorkFileService _workFileService;
    public ContentRepository(ILogger<ContentRepository> logger, VideoDbContext dbContext, IWorkFileService workFileService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _workFileService = workFileService;
    }
    public async Task<CachedContentSource?> GetContentSource(Guid videoId, Guid sourceId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos
            .Include(v => v.Sources)
            .FirstOrDefaultAsync(v => v.Id.ToLower() == videoId.ToString().ToLower(), cancellationToken);
        var source = video?.Sources?.FirstOrDefault((s) => s.Id.ToLower() == sourceId.ToString().ToLower());
        if (source is null) return null;
        var workSpace = await _workFileService.LoadWorkSpace(WorkSpaceDirectory.RepoDir, Guid.Parse(video!.WorkSpaceId));
        var directory = _workFileService.GetWorkSpaceDirectory(workSpace);
        var sourceLocation = Path.Join(directory, $"{source.Id}.mp4");
        if (!File.Exists(sourceLocation)) return null;
        byte[] data;
        await using (var fs = File.OpenRead(sourceLocation))
        {
            data = new byte[fs.Length];
            _ = await fs.ReadAsync(data.AsMemory(0, (int)fs.Length), cancellationToken);
        }
        return new CachedContentSource()
        {
            ContentType = source.ContentType,
            FileLocation = sourceLocation,
            SourceId = source.Id,
            VideoId = video.Id,
            Data = data
        };
    }
}