using System.Security.Claims;
using System.Threading.Channels;
using Domain.Constants;
using Domain.Entity;
using Domain.Model.Query;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Authorization;
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
    private readonly ChannelWriter<BackgroundTask> _channel;
    private readonly IVideoRepository _videoRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IUserRepository _userRepository;
    public VideoController(ILogger<VideoController> logger, 
        ChannelWriter<BackgroundTask> channel, 
        IUserRepository userRepository,
        IVideoRepository videoRepository,
        IPlaylistRepository playlistRepository)
    {
        _logger = logger;
        _channel = channel;
        _userRepository = userRepository;
        _videoRepository = videoRepository;
        _playlistRepository = playlistRepository;
    }

    [HttpPost("Query")]
    [AllowAnonymous]
    public async Task<IActionResult> QueryVideos([FromBody] QueryVideosRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var query = new VideoQuery()
        {
            Title = payload.Title,
            CreatorId = payload.CreatorId,
            From = 0,
            Count = 999,
            Status = payload.Status
        };
        if (payload.QueryUserVideos)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var user = await _userRepository.GetUserById(userId, cancellationToken);
            if (user is null)
            {
                return Unauthorized(new QueryVideosResponse()
                {
                    Success = false,
                    Message = "User is Unauthorized!",
                    Videos = new List<Video>(),
                    From = payload.From,
                    Count = payload.Count,
                    TotalCount = 0
                });
            }

            query.CreatorId = Guid.Parse(user.Id);
            query.IncludeUnlisted = true;
        }
        
        var totalCount =  await _videoRepository.QueryCountAsync(query, cancellationToken);
        query.From = payload.From;
        query.Count = payload.Count;
        var videos = await _videoRepository.QueryAsync(query, false, cancellationToken);
        return Ok(new QueryVideosResponse()
        {
            Success = true,
            Videos = videos,
            From = payload.From,
            Count = payload.Count,
            TotalCount = totalCount,
            Message = string.Empty
        });
    }
    
    [HttpPost("Playlist")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVideoPlaylist([FromBody] VideoPlaylistRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var query = new VideoQuery()
        {
            CreatorId = payload.CreatorId,
            AddSources = true,
            Status = VideoProcessingStatus.Published,
            OrderByCreated = payload.OrderByNewest,
            OrderByPopularity = payload.OrderByPopularity,
            PlaylistId = payload.PlaylistId,
            From = 0,
            Count = 999,
        };
        var totalCount = await _videoRepository.QueryCountAsync(query, cancellationToken);
        query.From = payload.From;
        query.Count = payload.Count;
        var videos = await _videoRepository.QueryAsync(query, true, cancellationToken);
        var creators = (await _userRepository.GetUsersByIds(videos.Select(v => v.CreatorId), cancellationToken)).ToLookup(v => v.Id);
        
        var playlistVideos = videos
            .Where(v => creators.Contains(v.CreatorId))
            .Select(v =>new VideoPlaylistModel(v, creators[v.CreatorId].First()));
        
        
        return Ok(new VideoPlaylistResponse()
        {
            Videos = playlistVideos,
            From = query.From,
            Count = query.Count,
            TotalCount = totalCount,
            Success = true
        });
    }
    
    [HttpGet("Metadata")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVideoMetadata([FromQuery] Guid videoId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        Guid? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is not null)
        {
            var user = await _userRepository.GetUserById(userIdClaim.Value, cancellationToken);
            userId = user is not null ? Guid.Parse(user.Id) : null;
        }
        
        var videoMetadata = await _videoRepository.GetVideoMetadataById(videoId, userId, cancellationToken);
        if (videoMetadata is null)
        {
            return NotFound(new Response()
            {
                Success = false,
                Message = "Video not found!"
            });
        }
        await _channel.WriteAsync(new VideoTask()
        {
            VideoId = videoId,
            WorkSpaceId = Guid.Empty,
            Type = BackgroundTaskType.IncrementVideoViewCount
        }, cancellationToken);
        return Ok(new VideoMetadataResponse()
        {
            Metadata = videoMetadata,
            Success = true
        });
    }
    
    [HttpPost("UpdateVideo")]
    public async Task<IActionResult> UpdateVideo([FromBody] UpdateVideoRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        
        var video = await _videoRepository.GetVideoById(payload.VideoId, false, cancellationToken);
        if (video is null || video?.CreatorId != user.Id)
        {
            return NotFound(new Response()
            {
                Success = false,
                Message = "Video not found!"
            });
        }

        video.Title = payload.Title;
        video.Description = payload.Description;
        video.IsUnlisted = payload.IsUnlisted;
        var videoId = await _videoRepository.UpdateVideo(video, cancellationToken);
        return Ok(new CreateOrUpdateVideoResponse()
        {
            VideoId = videoId,
            Message = string.Empty,
            Success = true
        });
    }
    
    [HttpPost("CreateVideo")]
    public async Task<IActionResult> CreateVideo([FromBody] CreateVideoRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        
        var videoId = await _videoRepository.CreateVideo(payload, Guid.Parse(user.Id), cancellationToken);
        await _channel.WriteAsync(new VideoTask()
        {
            VideoId = videoId,
            WorkSpaceId = payload.WorkSpaceId,
            Type = BackgroundTaskType.ProcessVideo
        }, cancellationToken);
        return Ok(new CreateOrUpdateVideoResponse()
        {
            VideoId = videoId,
            Message = string.Empty,
            Success = true
        });
    }
    
    [HttpGet("Sources")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVideoSources([FromQuery] Guid videoId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var sources = await _videoRepository.GetVideoSourcesById(videoId, cancellationToken);
        return Ok(sources);
    }
    
    [HttpPost("Impression")]
    public async Task<IActionResult> VideoImpression([FromBody] VideoImpressionRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        await _videoRepository.SetVideoImpression(Guid.Parse(user.Id), payload.VideoId, payload.Impression, cancellationToken);
        return Ok(new Response()
        {
            Success = true
        });
    }
    
    [HttpPost("ChangeVisibility")]
    public async Task<IActionResult> ChangeVideoVisibility([FromBody] ChangeVideoVisibilityRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        
        var video = await _videoRepository.GetVideoById(payload.VideoId, false, cancellationToken);
        if (video is null || video?.CreatorId != user.Id)
        {
            return NotFound(new Response()
            {
                Success = false,
                Message = "Video not found!"
            });
        }
        
        await _videoRepository.ChangeVisibility(payload.VideoId, payload.IsUnlisted, cancellationToken);
        if (!video.NotificationsSent)
        {
            await _channel.WriteAsync(new NotificationTask()
            {
                VideoId = payload.VideoId,
                Type = BackgroundTaskType.GenerateUploadNotifications
            }, cancellationToken);
        }
        return Ok(new Response()
        {
            Success = true
        });
    }
    
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteVideo([FromQuery] Guid videoId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        
        var video = await _videoRepository.GetVideoById(videoId, false, cancellationToken);
        if (video is null)
        {
            return NotFound(new Response()
            {
                Success = false,
                Message = "Video not found!"
            });
        }

        await _videoRepository.UpdateVideoStatus(videoId, VideoProcessingStatus.Deleting, cancellationToken);
        await _channel.WriteAsync(new VideoTask()
        {
            VideoId = Guid.Parse(video.Id),
            WorkSpaceId = Guid.Parse(video.WorkSpaceId),
            Type = BackgroundTaskType.DeleteVideo
        }, cancellationToken);
        return Ok(new Response()
        {
            Success = true
        });
    }
    
    [HttpPost("PublishVideo")]
    public async Task<IActionResult> PublishVideo([FromBody] PublishVideoRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }

        var video = await _videoRepository.GetVideoById(payload.VideoId, false, cancellationToken);
        if (video is null || video?.CreatorId != user.Id)
        {
            return NotFound(new Response()
            {
                Success = false,
                Message = "Video not found!"
            });
        }
        
        await _channel.WriteAsync(new VideoTask()
        {
            VideoId = Guid.Parse(video.Id),
            WorkSpaceId = Guid.Parse(video.WorkSpaceId),
            Type = BackgroundTaskType.PublishVideo
        }, cancellationToken);
        return Ok(new Response()
        {
            Success = true
        });
    }
    
    [HttpGet("Status")]
    public async Task<IActionResult> GetVideoStatus([FromQuery] Guid videoId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var video = await _videoRepository.GetVideoById(videoId, false, cancellationToken);
        if (video is null) {
            return NotFound(new VideoStatusResponse()
            {
                VideoId = videoId,
                Success = false,
                Message = "Video not found!"
            });
        }
        
        return Ok(new VideoStatusResponse()
        {
            VideoId = videoId,
            Status = video.Status,
            Success = true,
            Message = string.Empty
        });
    }

    [HttpGet("CreatorPlaylists")]
    [AllowAnonymous]
    public async Task<IActionResult> CreatorPlaylists([FromQuery] Guid creatorId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (creatorId == Guid.Empty)
        {
            return NotFound(new Response()
            {
                Success = false,
                Message = "Creator playlists not found!"
            });
        }
        var playlists = await _playlistRepository.GetPlaylistModels(creatorId, cancellationToken);
        return Ok(new CreatorPlaylistsResponse()
        {
            Success = true,
            CreatorId = creatorId,
            Playlists = playlists
        });
    }

    [HttpGet("UserPlaylists")]
    public async Task<IActionResult> UserPlaylists(CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }

        var creatorId = Guid.Parse(user.Id);
        var playlists = await _playlistRepository.GetPlaylistModels(creatorId, cancellationToken);
        return Ok(new CreatorPlaylistsResponse()
        {
            Success = true,
            CreatorId = creatorId,
            Playlists = playlists
        });
    }
    
    [HttpPost("AddOrRemovePlaylist")]
    public async Task<IActionResult> AddOrRemovePlaylist([FromBody] AddOrRemovePlaylistRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }

        if (payload.AddVideo)
        {
            await _playlistRepository.AddVideosToPlaylist(Guid.Parse(user.Id), payload.Videos, payload.PlaylistId, cancellationToken);
        }
        else
        {
            await _playlistRepository.RemoveVideosFromPlaylist(Guid.Parse(user.Id), payload.Videos, payload.PlaylistId, cancellationToken);    
        }
        return Ok(new Response()
        {
            Success = true
        });
    }
    
    [HttpPost("UpdatePlaylist")]
    public async Task<IActionResult> UpdatePlaylist([FromBody] UpdatePlaylistRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        
        await _playlistRepository.UpdatePlaylist(Guid.Parse(user.Id),  payload.PlaylistId, payload.Title, payload.Videos, cancellationToken);
        return Ok(new CreateOrUpdatePlaylistResponse()
        {
            Success = true
        });
    }
    [HttpDelete("DeletePlaylist")]
    public async Task<IActionResult> DeletePlaylist([FromQuery] Guid playlistId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        
        await _playlistRepository.DeletePlaylist(Guid.Parse(user.Id), playlistId, cancellationToken);
        return Ok(new Response()
        {
            Success = true
        });
    }
    [HttpPost("CreatePlaylist")]
    public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistRequest payload,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var user = await _userRepository.GetUserByClaimsPrincipal(User, cancellationToken);
        if (user is null) {
            return Unauthorized(new Response()
            {
                Success = false,
                Message = "User is Unauthorized!"
            });
        }
        var playlistId = await _playlistRepository.CreatePlaylist(Guid.Parse(user.Id), payload.Title, cancellationToken);
        if (playlistId == Guid.Empty)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new Response()
            {
                Success = false,
                Message = "Failed to create playlist, please try again later"
            });
        }

        if (payload.Videos.Count > 0)
        {
            await _playlistRepository.AddVideosToPlaylist(Guid.Parse(user.Id), payload.Videos, playlistId, cancellationToken);
        }

        return Ok(new CreateOrUpdatePlaylistResponse()
        {
            Success = true,
            PlaylistId = playlistId
        });
    }
}