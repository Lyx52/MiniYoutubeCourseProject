using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WebApp.Services.Interfaces;

namespace WebApp.Services;

public class JwtAuthenticationService : IAuthenticationService
{
    private readonly ILoginManager _loginManager;
    public JwtAuthenticationService(ILoginManager loginManager)
    {
        _loginManager = loginManager;
    }
    
    public async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
    {
        var claims = await _loginManager.GetUserClaimsAsync();
        if (claims.Count > 0)
        {
            var identity = new ClaimsPrincipal(new ClaimsIdentity(claims));
            return AuthenticateResult.Success(new AuthenticationTicket(identity, JwtBearerDefaults.AuthenticationScheme));
        }
        return AuthenticateResult.NoResult();
    }

    public async Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        var state = await AuthenticateAsync(context, scheme);
        if (state.Succeeded && state.Principal is not null) return;
        context.Response.Redirect("/");
    }

    public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }

    public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }

    public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        throw new NotImplementedException();
    }
}