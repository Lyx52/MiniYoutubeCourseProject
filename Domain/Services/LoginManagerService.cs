using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces;
using Domain.Model.Configuration;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;

namespace Domain.Services;

public class LoginManagerService : ILoginManager
{
    private const string AccessToken = "AccessToken";
    private const string RefreshToken = "RefreshToken";
    private readonly ProtectedLocalStorage _localStorage;
    private readonly NavigationManager _navigation;
    private readonly ILogger<LoginManagerService> _logger;
    private readonly AppConfiguration _configuration;
    private readonly IAuthHttpClient _authHttpClient;
    public LoginManagerService(
        ILogger<LoginManagerService> logger, 
        ProtectedLocalStorage localStorage,
        AppConfiguration configuration,
        NavigationManager navigation,
        IAuthHttpClient authHttpClient)
    {
        _localStorage = localStorage;
        _logger = logger;
        _navigation = navigation;
        _configuration = configuration;
        _authHttpClient = authHttpClient;
    }
    public async Task LogoutAsync(string redirectUri = "/", CancellationToken cancellationToken = default(CancellationToken))
    {
        await _authHttpClient.RevokeRefreshTokenAsync(this, cancellationToken);
        await _localStorage.DeleteAsync(AccessToken);
        await _localStorage.DeleteAsync(RefreshToken);
        _navigation.NavigateTo(redirectUri, true, true);
    }

    public async Task<LoginResponseModel> LoginAsync(LoginModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        var response = await _authHttpClient.LoginAsync(model, cancellationToken);
        if (response.Success)
        {
            await _localStorage.SetAsync(AccessToken, response.Token);
            await _localStorage.SetAsync(RefreshToken, response.RefreshToken);
        }

        return new LoginResponseModel()
        {
            Message = response.Message ?? string.Empty,
            Success = response.Success
        };
    }

    public async Task<List<Claim>> GetUserClaimsAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        var accessToken = await GetJwtToken(cancellationToken);
        
        if (accessToken is null) return new List<Claim>();
        if (JwtTokenHelper.TryValidateAndDecodeJwtToken(accessToken, _configuration.JWT, out var claims))
        {
            return claims.ToList();
        }
        
        // Try to refresh token if request failed
        var refreshResponse = await _authHttpClient.RefreshTokenAsync(this, cancellationToken);
        if (refreshResponse.Success &&
            JwtTokenHelper.TryValidateAndDecodeJwtToken(refreshResponse.Token, _configuration.JWT, out claims))
        {
            await _localStorage.SetAsync(AccessToken, refreshResponse.Token);
            await _localStorage.SetAsync(RefreshToken, refreshResponse.RefreshToken);
            return claims.ToList();
        }
        return new List<Claim>();
    }
    
    public async Task SetJwtToken(string token, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _localStorage.SetAsync(AccessToken, token);
    }

    public async Task<string?> GetJwtToken(CancellationToken cancellationToken = default(CancellationToken))
    {
        ProtectedBrowserStorageResult<string> accessToken;
        try
        {
            accessToken = await _localStorage.GetAsync<string>(AccessToken);
        }
        catch (Exception _)
        {
            // Can throw exceptions on page reloads
            return null;
        }

        return accessToken.Success ? accessToken.Value : null;
    }

    public async Task SetRefreshToken(string refreshToken, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _localStorage.SetAsync(RefreshToken, refreshToken);
    }

    public async Task<string?> GetRefreshToken(CancellationToken cancellationToken = default(CancellationToken))
    {
        ProtectedBrowserStorageResult<string> refreshToken;
        try
        {
            refreshToken = await _localStorage.GetAsync<string>(RefreshToken);
        }
        catch (Exception _)
        {
            // Can throw exceptions on page reloads
            return null;
        }

        return refreshToken.Success ? refreshToken.Value : null;
    }
}