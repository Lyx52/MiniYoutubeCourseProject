using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using Domain;
using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Configuration;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApi.Services.Interfaces;
using WebApi.Services.Models;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApiConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ChannelWriter<BackgroundTask> _channel;
    public AuthController(
        ILogger<AuthController> logger,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ApiConfiguration configuration,
        IUserRepository userRepository,
        ChannelWriter<BackgroundTask> channel)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
        _channel = channel;
    }

    [HttpPost]
    [Route("RevokeRefreshToken")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RevokeRefreshTokenRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }

        var success = await _userRepository.TryToRevokeRefreshToken(user.Id, payload.RefreshToken, cancellationToken);
        var response = new Response()
        {
            Success = success,
            Message = success ? string.Empty : "Invalid refresh token"
        };
        return success ? Ok(response) : BadRequest(response);
    }
    
    [HttpPost]
    [Route("RefreshToken")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var claims = JwtTokenHelper.ReadJwtTokenClaims(payload.ExpiredToken).ToList();
        var userName = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        var userId = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new RefreshTokenResponse()
            {
                Success = false,
                Message = "Provided JWT token is invalid"
            });
        }
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null || user.Id != userId)
        {
            return Unauthorized(new RefreshTokenResponse()
            {
                Success = false,
                Message = "Provided JWT token is invalid"
            });
        }

        var newRefreshToken = await _userRepository.RegenerateRefreshToken(user.Id, payload.RefreshToken, cancellationToken);
        if (string.IsNullOrEmpty(newRefreshToken))
        {
            return Unauthorized(new RefreshTokenResponse()
            {
                Success = false,
                Message = "Provided RefreshToken is invalid or expired"
            });    
        }
        var newToken = JwtTokenHelper.GenerateJwtToken(user.UserName!, user.Id, _configuration.JWT);
        var newJwtToken = new JwtSecurityTokenHandler().WriteToken(newToken);
        return Ok(new RefreshTokenResponse()
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken,
            BearerToken = $"Bearer {newJwtToken}",
            Expiration = newToken.ValidTo,
            Success = true
        });
    }
    
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userManager.FindByNameAsync(payload.Username);
        if (user is null || !await _userManager.CheckPasswordAsync(user, payload.Password))
        {
            return Unauthorized(new LoginResponse()
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new LoginResponse()
            {
                Success = false,
                Message = "Email not confirmed!"
            });
        }
        
        var token = JwtTokenHelper.GenerateJwtToken(user.UserName!, user.Id, _configuration.JWT);
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = await _userRepository.GenerateRefreshToken(user.Id, cancellationToken);
        return Ok(new LoginResponse()
        {
            Token = jwtToken,
            RefreshToken = refreshToken,
            BearerToken = $"Bearer {jwtToken}",
            Expiration = token.ValidTo,
            Success = true
        });

    }

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userExists = await _userManager.FindByNameAsync(payload.Username);
        if (userExists is not null)
        {
            return BadRequest(new Response()
            {
                Message = "User with this username already exists!",
                Success = false
            });
        }
        userExists = await _userManager.FindByEmailAsync(payload.Email);
        if (userExists is not null)
        {
            return BadRequest(new Response()
            {
                Message = "User with this email already exists!",
                Success = false
            });
        }
        var user = new User()
        {
            Email = payload.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = payload.Username,
            CreatorName = payload.Username,
            EmailConfirmed = true,
        };
        var result = await _userManager.CreateAsync(user, payload.Password);
        // var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //
        // await _channel.WriteAsync(new SendConfirmationTask()
        // {
        //     Email = payload.Email,
        //     Token = token,
        //     Type = BackgroundTaskType.SendConfirmationEmail
        // }, cancellationToken);
        return Ok(new Response()
        {
            Success = result.Succeeded,
            Message = result.Succeeded ? string.Empty : string.Join(',', result.Errors.Select(e => e.Code))
        });
    }

    [HttpGet]
    [Route("ConfirmUser")]
    public async Task<IActionResult> ConfirmUser([FromQuery] string token, [FromQuery] string email, CancellationToken cancellationToken = default(CancellationToken))
    {
        var redirectAddress = _configuration.Endpoints
            .First(c => c.Name == EndpointNames.MiniTubeApp.ToString()).Hostname;
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return Redirect($"{redirectAddress}/confirmation?success=false");
        if (await _userManager.IsEmailConfirmedAsync(user)) return Redirect(redirectAddress);
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return Redirect($"{redirectAddress}/confirmation?success={result.Succeeded}");
    }
    
    [HttpGet]
    [Route("Profile")]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new UserProfileResponse()
            {
                Success = false,
                Message = "User is Unauthorized"
            });
        }
        return Ok(new UserProfileResponse()
        {
            User = user,
            Success = true
        });
    }
    
    
}