using System.Security.Claims;
using Domain.Entity;
using Domain.Model.Request;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface IUserRepository
{
    Task<string> GenerateRefreshToken(string userId,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> TryToRevokeRefreshToken(string userId, string refreshToken,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<string?> RegenerateRefreshToken(string userId, string refreshToken,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<UserModel>> GetUsersByIds(IEnumerable<string> userIds,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<UserModel?> GetUserById(string userId, CancellationToken cancellationToken = default(CancellationToken));
    Task<UserModel?> GetUserByClaimsPrincipal(ClaimsPrincipal user, CancellationToken cancellationToken = default(CancellationToken));
    Task UpdateProfile(string userId, UpdateUserProfileRequest payload, CancellationToken cancellationToken = default(CancellationToken));
}