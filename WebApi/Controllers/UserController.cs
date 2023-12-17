using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApiConfiguration _configuration;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly INotificationRepository _notificationRepository;
    public UserController(
        ILogger<UserController> logger,
        ApiConfiguration configuration,
        IUserRepository userRepository,
        ISubscriberRepository subscriberRepository,
        INotificationRepository notificationRepository)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
        _subscriberRepository = subscriberRepository;
        _notificationRepository = notificationRepository;
    }

    [HttpPost]
    [Route("Subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
        if (user is null) return Unauthorized();
        
        await _subscriberRepository.Subscribe(user.Id, payload.CreatorId, cancellationToken);
        return Ok();
    }
    
    [HttpPost]
    [Route("Unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
        if (user is null) return Unauthorized();
        
        await _subscriberRepository.Unsubscribe(user.Id, payload.CreatorId, cancellationToken);
        return Ok();
    }
    
    [HttpGet]
    [Route("Profile")]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
        if (user is null) return Unauthorized();
        return Ok(new UserProfileResponse()
        {
            User = user,
            Success = true
        });
    }
    
    [HttpGet]
    [Route("Notifications")]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
        if (user is null) return Unauthorized();
        var notifications = await _notificationRepository.GetUserNotifications(user.Id, cancellationToken);
        return Ok(new UserNotificationResponse()
        {
            UserId = user.Id,
            Notifications = notifications,
            Success = true
        });
    }
    
    [HttpPost]
    [Route("DismissNotification")]
    public async Task<IActionResult> DismissNotification([FromBody] DismissNotificationRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
        if (user is null) return Unauthorized();
        
        await _notificationRepository.DismissNotification(user.Id, payload.NotificationId, cancellationToken);
        return Ok();
    }
    
    [HttpGet]
    [Route("CreatorProfile")]
    [AllowAnonymous]
    public async Task<IActionResult> CreatorProfile(string creatorId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        UserModel? user = null;
        if (userIdClaim is not null)
        {
            user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
        }
        
        var creator = await _userRepository.GetUserById(creatorId, cancellationToken);
        if (creator is null) return NotFound();
        creator.Email = string.Empty;
        creator.Username = string.Empty;
        
        bool isSubscribed = false;
        if (user is not null)
        {
            isSubscribed = await _subscriberRepository.IsSubscribed(user.Id, creator.Id, cancellationToken);
        }

        var subscriberCount = await _subscriberRepository.GetSubscriberCount(creator.Id, cancellationToken);
        return Ok(new CreatorProfileResponse()
        {
            SubscriberCount = subscriberCount,
            Creator = creator,
            IsSubscribed = isSubscribed,
            Success = true
        });
    }
    [HttpGet]
    [Route("PublicProfile")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicProfile(string userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserById(userId, cancellationToken);
        if (user is null) return NotFound();
        user.Email = string.Empty;
        user.Username = string.Empty;
        return Ok(new UserProfileResponse()
        {
            User = user,
            Success = true
        });
    }
}