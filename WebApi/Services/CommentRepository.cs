﻿using Domain.Constants;
using Domain.Entity;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class CommentRepository : ICommentRepository
{
    private readonly ILogger<CommentRepository> _logger;
    private readonly VideoDbContext _dbContext;
    public CommentRepository(ILogger<CommentRepository> logger, VideoDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string> CreateComment(string userId, string videoId, string message, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);
        if (video is null) return string.Empty;
        var id = Guid.NewGuid();
        var result = await _dbContext.Comments.AddAsync(new Comment()
        {
            Id = id.ToString(),
            VideoId = videoId,
            Video = video,
            Message = message,
            Dislikes = 0,
            Likes = 0,
            CommenterId = userId,
            Created = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return id.ToString();
    }

    public async Task<IEnumerable<CommentModel>> GetByVideoIds(string videoId, string? userId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos
            .Include(v => v.Comments) 
            .ThenInclude(c => c.Impressions)
            .FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);
        var comments = new List<CommentModel>();
        if (video is null) return comments;
        foreach (var comment in video.Comments)
        {
            var totalDislikes = comment.Impressions.Count(i => i.Impression == ImpressionType.Dislike);
            var totalLikes = comment.Impressions.Count(i => i.Impression == ImpressionType.Like);
            var model = new CommentModel()
            {
                Message = comment.Message,
                Created = comment.Created,
                Dislikes = totalDislikes,
                Likes = totalLikes,
                UserId = comment.CommenterId,
                Id = comment.Id,
                Impression = ImpressionType.None
            };
            comments.Add(model);
            if (string.IsNullOrEmpty(userId)) continue;
            var compositeId = $"{userId[0..18]}-{comment.Id[19..36]}";
            model.Impression = comment.Impressions.FirstOrDefault(i => i.Id == compositeId)?.Impression ?? ImpressionType.None;
        }
        
        return comments;
    }

    public async Task SetCommentImpression(string userId, string commentId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var compositeId = $"{userId[0..18]}-{commentId[19..36]}";
        var impression =
            await _dbContext.CommentImpressions.FirstOrDefaultAsync(
                ci => ci.Id == compositeId, cancellationToken);
        if (impression is not null)
        {
            impression.Impression = impressionType;
        }
        else
        {
            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
            if (comment is null) return;
            await _dbContext.CommentImpressions.AddAsync(new CommentImpression()
            {
                Id = compositeId,
                Impression = impressionType,
                CommentId = commentId,
                Comment = comment
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}