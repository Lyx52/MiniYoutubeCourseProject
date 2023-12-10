using System.Security.Claims;
using System.Threading.Channels;
using Domain.Constants;
using Domain.Entity;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    private readonly ChannelWriter<VideoTask> _channel;
    private readonly UserManager<User> _userManager;
    private readonly IVideoRepository _videoRepository;
    private static readonly IEnumerable<Video> EmptyVideos = new List<Video>();
    public VideoController(ILogger<VideoController> logger, 
        ChannelWriter<VideoTask> channel, 
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
        await _channel.WriteAsync(new VideoTask()
        {
            VideoId = videoId,
            WorkSpaceId = payload.WorkSpaceId,
            Type = VideoTaskType.ProcessVideo
        }, cancellationToken);
        return Ok(new CreateVideoResponse()
        {
            VideoId = videoId.ToString(),
            Message = string.Empty,
            Success = true
        });
    }

    [HttpPost("PublishVideo")]
    public async Task<IActionResult> PublishVideo([FromBody] PublishVideoRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(userIdClaim.Value);
        if (user is null) return Unauthorized();
        var video = await _videoRepository.GetVideoById(payload.VideoId, user, cancellationToken);
        if (video is null) return NotFound();
        await _channel.WriteAsync(new VideoTask()
        {
            VideoId = Guid.Parse(video.Id),
            WorkSpaceId = Guid.Parse(video.WorkSpaceId),
            Type = VideoTaskType.PublishVideo
        }, cancellationToken);
        return Ok();
    }
    
    [HttpPost("Status")]
    public async Task<IActionResult> GetVideoStatus([FromBody] VideoStatusRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var status = await _videoRepository.GetVideoStatus(payload.VideoId, cancellationToken);
        if (!status.HasValue) return BadRequest();
        return Ok(new VideoStatusResponse()
        {
            VideoId = payload.VideoId,
            Status = status.Value ,
            Success = true,
            Message = string.Empty
        });
    }
    
    [HttpPost("Playlist")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVideoPlaylist([FromBody] VideoPlaylistRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var videos = await _videoRepository.GetVideoPlaylist(payload.From, payload.Count, cancellationToken);
        return Ok(new VideoPlaylistResponse()
        {
            Videos = videos,
            Success = true,
            Message = string.Empty
        });
    }
    
    [HttpGet("Metadata")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVideoMetadata([FromQuery] string id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            if (Guid.TryParse(id, out var videoId))
            {
                var video = await _videoRepository.GetVideoById(videoId, true, cancellationToken);
                if (video is null) return NotFound();
                return Ok(new VideoMetadataResponse()
                {
                    VideoId = video.Id,
                    Description = video.Description,
                    Title = video.Title,
                    ContentSources = video.Sources?.Select((s) => new ContentSourceModel()
                    {
                        Id = s.Id,
                        Resolution = s.Resolution,
                        Type = s.Type,
                        ContentType = s.ContentType
                    }) ?? new List<ContentSourceModel>(),
                    Success = true
                });
            }
            
            return BadRequest(new VideoMetadataResponse()
            {
                Success = false,
                Message = "Invalid videoId"
            });
        }
        catch (Exception e)
        {
            _logger.LogError("Caught exception while querying videos {ExceptionMessage}", e.Message);
            return StatusCode(500, new VideoMetadataResponse()
            {
                Success = false,
                Message = "Failed to query videos, please try again later",
            });
        }
    }

    [HttpGet("Sources")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVideoSources([FromQuery] string id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (Guid.TryParse(id, out var videoId))
        {
            var sources = await _videoRepository.GetVideoSourcesById(videoId, cancellationToken);
            return Ok(sources);
        }

        return BadRequest("Invalid video id");
    }
    
    [HttpGet("Query")]
    [AllowAnonymous]
    public async Task<IActionResult> QueryVideos([FromQuery] string searchText,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            var videos = await _videoRepository.QueryVideosByTitle(searchText, cancellationToken);
            return Ok(new SearchVideosResponse()
            {
                Videos = videos,
                Success = true,
            });
        }
        catch (Exception e)
        {
            _logger.LogError("Caught exception while querying videos {ExceptionMessage}", e.Message);
            return StatusCode(500, new SearchVideosResponse()
            {
                Success = false,
                Message = "Failed to query videos, please try again later",
                Videos = EmptyVideos
            });
        }
    }
}