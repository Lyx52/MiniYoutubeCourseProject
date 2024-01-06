using System.Security.Claims;
using Domain.Constants;
using Domain.Entity;
using Domain.Model.Request;
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
    private readonly UserDbContext _userDbContext;
    public const int MaxRefreshTokenRegenerations = 3;
    public UserRepository(ILogger<UserRepository> logger, UserManager<User> userManager, UserDbContext userDbContext)
    {
        _userManager = userManager;
        _logger = logger;
        _userDbContext = userDbContext;
    }

    public async Task<bool> TryToRevokeRefreshToken(string userId, string refreshToken,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var token = await _userDbContext.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.UserId == userId && rt.Token == refreshToken, cancellationToken);
        if (token is null) return false;
        token.ExpiryDate = DateTime.UtcNow;
        token.IsRevoked = true;
        await _userDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string?> RegenerateRefreshToken(string userId, string refreshToken,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var token = await _userDbContext.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.UserId == userId && rt.Token == refreshToken, cancellationToken);
        if (token is null) return null;
        if (token.RefreshCount >= MaxRefreshTokenRegenerations || DateTime.UtcNow > token.ExpiryDate) return null;
        token.Token = Guid.NewGuid().ToString();
        token.RefreshCount++;
        await _userDbContext.SaveChangesAsync(cancellationToken);
        return token.Token;
    }
    public async Task<string> GenerateRefreshToken(string userId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var token = Guid.NewGuid().ToString();
        await _userDbContext.RefreshTokens.AddAsync(new RefreshToken()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            RefreshCount = 0
        }, cancellationToken);
        await _userDbContext.SaveChangesAsync(cancellationToken);
        return token;
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

    public async Task UpdateProfile(string userId, UpdateUserProfileRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;
        user.CreatorName = payload.CreatorName;
        user.Icon = payload.IconLink;
        await _userManager.UpdateAsync(user);
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