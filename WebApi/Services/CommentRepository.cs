using Domain.Constants;
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

    public async Task<IEnumerable<CommentModel>> GetByVideoIds(string videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _dbContext.Videos
            .Include(v => v.Comments)
            .FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);
        return video?.Comments.Select(c => new CommentModel()
        {
            Message = c.Message,
            Created = c.Created,
            Dislikes = c.Dislikes,
            Likes = c.Likes,
            UserId = c.CommenterId
        }) ?? new List<CommentModel>();
    }

    public async Task LikeDislikeComment(string id, bool isLike,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (isLike)
        {
            comment.Likes++;
        } else comment.Dislikes++;
    }
}