using Domain.Entity;
using Domain.Model.View;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class SubscriberRepository : ISubscriberRepository
{
    private readonly ILogger<SubscriberRepository> _logger;
    private readonly IUserRepository _userRepository;
    private readonly VideoDbContext _dbContext;
    public SubscriberRepository(ILogger<SubscriberRepository> logger, IUserRepository userRepository, VideoDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<Subscriber>> GetSubscribers(string creatorId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await _dbContext.Subscribers
            .Where(s => s.CreatorId == creatorId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<UserModel>> GetSubscribedCreators(string userId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var creators = await _dbContext.Subscribers
            .Where(s => s.SubscriberId == userId)
            .ToListAsync(cancellationToken);
        return await _userRepository
            .GetUsersByIds(creators.Select(c => c.CreatorId), cancellationToken);
    }

    public async Task Subscribe(string userId, string creatorId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (await IsSubscribed(userId, creatorId, cancellationToken)) return;
        await _dbContext.Subscribers.AddAsync(new Subscriber()
        {
            Id = Guid.NewGuid().ToString(),
            CreatorId = creatorId,
            SubscriberId = userId
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task Unsubscribe(string userId, string creatorId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var subscription = await _dbContext.Subscribers.FirstOrDefaultAsync(s => s.CreatorId == creatorId && s.SubscriberId == userId,
            cancellationToken);
        if (subscription is null) return;
        _dbContext.Subscribers.Remove(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsSubscribed(string userId, string creatorId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Subscribers
            .AnyAsync(s => s.CreatorId == creatorId && s.SubscriberId == userId, cancellationToken);
    }
    public Task<long> GetSubscriberCount(string creatorId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return _dbContext.Subscribers
            .LongCountAsync(s => s.CreatorId == creatorId, cancellationToken);
    }
}