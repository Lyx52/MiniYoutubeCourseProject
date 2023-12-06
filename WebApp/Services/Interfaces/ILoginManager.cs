using System.Security.Claims;
using Domain.Model.Response;

namespace WebApp.Services.Interfaces;

public interface ILoginManager
{
    Task LogoutAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<bool> LoginAsync(LoginResponse payload, CancellationToken cancellationToken = default(CancellationToken));
    Task<List<Claim>> GetUserClaimsAsync(CancellationToken cancellationToken = default(CancellationToken));
}