using Domain.Constants;
using Domain.Entity;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly UserManager<User> _userManager;
    public UserRepository(ILogger<UserRepository> logger, UserManager<User> userManager)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<UserModel>> GetUsersByIds(IEnumerable<string> userIds, CancellationToken cancellationToken = default(CancellationToken))
    {
        return await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserModel()
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                IconLink = u.Icon ?? string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetById(string userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return _userManager.FindByIdAsync(userId);
    }
}