using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class NotificationRepository : INotificationRepository
{
    private readonly ILogger<NotificationRepository> _logger;
    private readonly ApplicationDbContext _dbContext;
    public NotificationRepository(ILogger<NotificationRepository> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    public async Task<Guid> AddNotification(string userId, string message, string link, CancellationToken cancellationToken = default(CancellationToken))
    {
        var id = Guid.NewGuid();
        await _dbContext.Notifications.AddAsync(new UserNotification()
        {
            Id = id.ToString(),
            Message = message,
            UserId = userId,
            RedirectLink = link,
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<IEnumerable<UserNotification>> GetUserNotifications(string userId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync<UserNotification>(cancellationToken);
    }

    public async Task DismissNotification(string userId, string notificationId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);
        if (notification is null) return;
        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}