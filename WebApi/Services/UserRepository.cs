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

public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly UserManager<User> _userManager;
    public UserRepository(ILogger<UserRepository> logger, UserManager<User> userManager)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<UserModel>> GetUsersByIds(IEnumerable<string> userIds,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => AsPublicUser(u)!)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserModel?> GetUserById(string userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return AsPublicUser(await _userManager.FindByIdAsync(userId));
    }

    public Task<UserModel?> GetUserByClaimsPrincipal(ClaimsPrincipal user, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId is null ? Task.FromResult(default(UserModel)) : GetUserById(userId, cancellationToken);
    }

    private static UserModel? AsPublicUser(User? user)
    {
        if (user is null) return null;
        return new UserModel()
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            IconLink = user.Icon ?? string.Empty,
            Email = user.Email ?? string.Empty,
            CreatorName = user.CreatorName ?? user.UserName ?? string.Empty
        };
    }
}