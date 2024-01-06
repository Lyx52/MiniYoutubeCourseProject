using Domain.Model;
using Domain.Model.Response;
using Domain.Model.View;

namespace Domain.Interfaces;

public interface IAuthHttpClient
{
    Task<LoginResponse> LoginAsync(LoginModel model, CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default(CancellationToken));
    Task<RefreshTokenResponse> RefreshTokenAsync(ILoginManager loginManager,
        CancellationToken cancellationToken = default(CancellationToken));
    Task<Response> RevokeRefreshTokenAsync(ILoginManager loginManager,
        CancellationToken cancellationToken = default(CancellationToken));
}