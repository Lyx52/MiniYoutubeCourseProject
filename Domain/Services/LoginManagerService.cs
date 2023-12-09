using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces;
using Domain.Model.Configuration;
using Domain.Model.View;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
    public async Task LogoutAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        await _localStorage.DeleteAsync(AccessToken);
        _navigation.NavigateTo("/", true);
    }

    public async Task<LoginResponseModel> LoginAsync(LoginModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        var response = await _authHttpClient.LoginAsync(model, cancellationToken);
        if (response.Success && !string.IsNullOrEmpty(response.Token))
            await _localStorage.SetAsync(AccessToken, response.Token);
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
        var claims = ValidateDecodeToken(accessToken);
        if (claims.Count > 0) return claims;
        
        // TODO: Refresh token
        return new List<Claim>();
    }

    public async Task<string?> GetJwtToken(CancellationToken cancellationToken)
    {
        ProtectedBrowserStorageResult<string> accessToken;
        try
        {
            accessToken = await _localStorage.GetAsync<string>(AccessToken);
        }
        catch (InvalidOperationException _)
        {
            // Blazor first render cant access LocalStorage
            return null;
        }
        catch (CryptographicException)
        {
            return null;
        }

        return accessToken.Success ? accessToken.Value : null;
    }

    private List<Claim> ValidateDecodeToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidIssuer = _configuration.JWT.ValidIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.JWT.Secret))
            }, out _);
        }
        catch
        {
            return new List<Claim>();
        }

        var securityToken = tokenHandler.ReadJwtToken(token);
        return securityToken?.Claims.ToList() ?? new List<Claim>();
    }
}