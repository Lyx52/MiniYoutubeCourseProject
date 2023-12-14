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
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ApiConfiguration _configuration;
    private readonly ILogger<CommentController> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    public CommentController(
        ILogger<CommentController> logger,
        ApiConfiguration configuration,
        IVideoRepository videoRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _videoRepository = videoRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }
    
    [HttpPost("Impression")]
    public async Task<IActionResult> CommentImpression(CommentImpressionRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userRepository.GetById(userIdClaim.Value, cancellationToken);
        if (user is null) return Unauthorized();
        await _commentRepository.SetCommentImpression(user.Id, payload.CommentId, payload.Impression, cancellationToken);
        return Ok();
    }

        
    [HttpPost("Create")]
    public async Task<IActionResult> CreateComment(CreateCommentRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userRepository.GetById(userIdClaim.Value, cancellationToken);
        if (user is null) return Unauthorized();
        var video = await _videoRepository.GetVideoById(payload.VideoId, false, cancellationToken);
        if (video is null) return NotFound();
        
        var commentId = await _commentRepository.CreateComment(user.Id, video.Id, payload.Message, cancellationToken);
        if (string.IsNullOrEmpty(commentId))
            return StatusCode(StatusCodes.Status500InternalServerError, new Response()
            {
                Success = false,
                Message = "Failed to create comment, please try again later"
            });
        return Ok(new Response()
        {
            Success = true
        });
    }

    [HttpGet("Query")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommentsByVideoId([FromQuery] string id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            string? userId = string.Empty;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is not null)
            {
                var user = await _userRepository.GetById(userIdClaim.Value, cancellationToken);
                userId = user?.Id;
            }
            var comments = (await _commentRepository.GetByVideoIds(id, userId, cancellationToken)).ToList();
            var userIds = comments.Select(c => c.UserId).Distinct();
            var users = await _userRepository.GetUsersByIds(userIds, cancellationToken);
            return Ok(new QueryCommentsResponse()
            {
                Comments = comments,
                Users = users,
                Success = true
            });
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to fetch comments for {VideoId}", id);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new Response()
        {
            Success = false,
            Message = "Failed to query comments, please try again later",
        });
    }
}