using System.Security.Claims;
using System.Threading.Channels;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services.Interfaces;
using WebApi.Services.Models;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VideoController : ControllerBase
{
    private readonly ILogger<VideoController> _logger;
    private readonly ChannelWriter<ProcessVideoTask> _channel;
    private readonly UserManager<User> _userManager;
    private readonly IVideoRepository _videoRepository;
    public VideoController(ILogger<VideoController> logger, 
        ChannelWriter<ProcessVideoTask> channel, 
        UserManager<User> userManager,
        IVideoRepository videoRepository)
    {
        _logger = logger;
        _channel = channel;
        _userManager = userManager;
        _videoRepository = videoRepository;
    }
    
    [HttpPost("CreateVideo")]
    public async Task<IActionResult> CreateVideo([FromBody] CreateVideoRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null) return Unauthorized();
        var videoId = await _videoRepository.CreateVideo(payload, user, cancellationToken);
        await _channel.WriteAsync(new ProcessVideoTask()
        {
            VideoId = videoId,
            WorkSpaceId = payload.WorkSpaceId
        }, cancellationToken);
        return Ok(new CreateVideoResponse()
        {
            VideoId = videoId.ToString()
        });
    }

    [HttpGet("Video")]
    public async Task<IActionResult> GetVideoMetadata([FromQuery] string id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (Guid.TryParse(id, out var videoId))
        {
            var video = await _videoRepository.GetVideoById(videoId, true, cancellationToken);
            if (video is null) return NotFound();
            return Ok(video);
        }

        return BadRequest("Invalid video id");
    }
}