using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using WebApp.Services.Interfaces;

namespace WebApp.Services;
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    
    private static readonly AuthenticationState NotAuthenticatedState = new AuthenticationState(new ClaimsPrincipal());
    private readonly ILoginManager _loginManager;
    public JwtAuthStateProvider(ILoginManager loginManager)
    {
        _loginManager = loginManager;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = await _loginManager.GetUserClaimsAsync();
        if (claims.Count <= 0) return NotAuthenticatedState;
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme)));
    }
}