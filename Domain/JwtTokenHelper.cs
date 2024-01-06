using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Model.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Domain;

public static class JwtTokenHelper
{
    public static JwtSecurityToken GenerateJwtToken(string userName, string userId, JWTConfiguration configuration)
    {
        var authClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId)
        };
        
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Secret));

        var token = new JwtSecurityToken(
            issuer: configuration.ValidIssuer,
            audience: configuration.ValidAudience,
            expires: DateTime.UtcNow.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public static IEnumerable<Claim> ReadJwtTokenClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token)) return new List<Claim>();
        var securityToken = tokenHandler.ReadJwtToken(token);
        return securityToken is null ? new List<Claim>() : securityToken.Claims;
    }
    
    public static bool TryValidateAndDecodeJwtToken(string token, JWTConfiguration configuration, out IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        claims = new List<Claim>();
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidIssuer = configuration.ValidIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Secret))
            }, out var validatedToken);
        }
        
        catch (Exception)
        {
            return false;
        }

        if (!tokenHandler.CanReadToken(token)) return false;
        var securityToken = tokenHandler.ReadJwtToken(token);
        if (securityToken is null) return false;
        claims = securityToken.Claims;
        return true;
    }
}