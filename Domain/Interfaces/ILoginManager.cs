using System.Security.Claims;
using Domain.Model.Response;
using Domain.Model.View;

namespace Domain.Interfaces;

public interface ILoginManager
{
    Task LogoutAsync(string redirectUri = "/", CancellationToken cancellationToken = default(CancellationToken));
    Task<LoginResponseModel> LoginAsync(LoginModel model, CancellationToken cancellationToken = default(CancellationToken));
    Task<List<Claim>> GetUserClaimsAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<string?> GetJwtToken(CancellationToken cancellationToken = default(CancellationToken));
    Task<string?> GetRefreshToken(CancellationToken cancellationToken = default(CancellationToken));
    Task SetJwtToken(string token, CancellationToken cancellationToken = default(CancellationToken));
    Task SetRefreshToken(string refreshToken, CancellationToken cancellationToken = default(CancellationToken));
}