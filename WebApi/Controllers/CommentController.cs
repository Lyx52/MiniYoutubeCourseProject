using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Configuration;
using Domain.Model.Query;
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
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ILogger<CommentController> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    public CommentController(
        ILogger<CommentController> logger,
        IVideoRepository videoRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }
    
    [HttpPost("Impression")]
    public async Task<IActionResult> CommentImpression([FromBody] CommentImpressionRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var user = await _userRepository.GetUserById(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        await _commentRepository.SetCommentImpression(Guid.Parse(user.Id), payload.CommentId, payload.Impression, cancellationToken);
        return  Ok(new Response()
        {
            Success = true,
        });
    }

        
    [HttpPost("Create")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest payload,
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
        var video = await _videoRepository.GetVideoById(payload.VideoId, false, cancellationToken);
        if (video is null)
        {
            return NotFound(new Response()
            {
                Success = false,
                Message = "Video not found!"
            }); 
        }
        var commentId = await _commentRepository.CreateComment(Guid.Parse(user.Id), payload.VideoId, payload.Message, cancellationToken);
        if (string.IsNullOrEmpty(commentId))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Response()
            {
                Message = "Failed to create comment",
                Success = false,
            });
        }
        return  Ok(new Response()
        {
            Success = true,
        });
    }

    [HttpGet("Query")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommentsByVideoId([FromQuery] Guid videoId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        Guid? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is not null)
        {
            var user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
            userId = Guid.TryParse(user?.Id, out var uid) ? uid : null;
        }
            
        var comments = (await _commentRepository.GetByVideoIds(videoId, userId, cancellationToken)).ToList();
        var userIds = comments.Select(c => c.UserId).Distinct();
        var users = await _userRepository.GetUsersByIds(userIds, cancellationToken);
        return Ok(new QueryCommentsResponse()
        {
            Comments = comments,
            Users = users,
            Success = true
        });
    }
}