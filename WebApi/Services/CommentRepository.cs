using Domain.Constants;
using Domain.Entity;
using Domain.Model.Query;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class CommentRepository : ICommentRepository
{
    private readonly ILogger<CommentRepository> _logger;
    private readonly ApplicationDbContext _dbContext;
    public CommentRepository(ILogger<CommentRepository> logger, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string> CreateComment(Guid userId, Guid videoId, string message, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken);
        if (video is null) return string.Empty;
        var id = Guid.NewGuid();
        var result = await _dbContext.Comments.AddAsync(new Comment()
        {
            Id = id.ToString(),
            VideoId = videoId.ToString(),
            Video = video,
            Message = message,
            Dislikes = 0,
            Likes = 0,
            CommenterId = userId.ToString(),
            Created = DateTime.UtcNow
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return id.ToString();
    }

    public async Task<IEnumerable<CommentModel>> GetByVideoIds(Guid videoId, Guid? userId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos
            .Include(v => v.Comments) 
            .ThenInclude(c => c.Impressions)
            .FirstOrDefaultAsync(v => v.Id == videoId.ToString(), cancellationToken);
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
                VideoId = comment.VideoId,
                Id = comment.Id,
                UserImpression = ImpressionType.None
            };
            comments.Add(model);
            if (!userId.HasValue) continue;
            var compositeId = $"{userId.Value.ToString()[0..18]}-{comment.Id[19..36]}";
            model.UserImpression = comment.Impressions.FirstOrDefault(i => i.Id == compositeId)?.Impression ?? ImpressionType.None;
        }
        
        return comments;
    }

    public async Task SetCommentImpression(Guid userId, Guid commentId, ImpressionType impressionType,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var compositeId = $"{userId.ToString()[0..18]}-{commentId.ToString()[19..36]}";
        var impression =
            await _dbContext.CommentImpressions.FirstOrDefaultAsync(
                ci => ci.Id == compositeId, cancellationToken);
        if (impression is not null)
        {
            impression.Impression = impressionType;
        }
        else
        {
            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId.ToString(), cancellationToken);
            if (comment is null) return;
            await _dbContext.CommentImpressions.AddAsync(new CommentImpression()
            {
                Id = compositeId,
                Impression = impressionType,
                CommentId = commentId.ToString(),
                Comment = comment
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}