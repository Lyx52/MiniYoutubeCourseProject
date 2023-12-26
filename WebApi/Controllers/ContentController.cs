using Domain.Constants;
using Domain.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApi.Attributes;
using WebApi.Services.Interfaces;
using static Domain.Constants.ValidFileExtensions;

namespace WebApi.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly ILogger<ContentController> _logger;
    private readonly IContentService _contentService;
    private readonly IContentRepository _contentRepository;
    private readonly IMemoryCache _cache;

    private static readonly Dictionary<ContentSourceType, MemoryCacheEntryOptions> CacheOptions = new ()
    {
        {
            ContentSourceType.Thumbnail, 
            new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(15)
            }
        },
        {
            ContentSourceType.Video, 
            new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(60)
            }
        },
        {
            ContentSourceType.ThumbnailGif, 
            new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(15)
            }
        }
    };
    public ContentController(
        ILogger<ContentController> logger, 
        IContentService contentService, 
        IContentRepository contentRepository,
        IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
        _contentService = contentService;
        _contentRepository = contentRepository;
    }

    [HttpPost("UploadVideoFile")]
    [FileExtensionValidation(MP4, MP4, WEBM)]
    [RequestSizeLimit(int.MaxValue - 1)]
    public async Task<IActionResult> UploadVideoFile([FromForm] IFormFile videoFile, CancellationToken cancellationToken = default(CancellationToken))
    {
        MemoryStream memoryStream = new MemoryStream();
        await videoFile.OpenReadStream().CopyToAsync(memoryStream, cancellationToken);
        var result = await _contentService.SaveTemporaryFile(memoryStream, videoFile.FileName, cancellationToken);
        if (result.Success)
        {
            return Ok(new UploadVideoFileResponse()
            {
                FileId = result.WorkSpaceId!.Value,
                FileName = $"{result.WorkSpaceId!}{Path.GetExtension(videoFile.FileName)}",
                Success = true,
            });
        }
        
        return StatusCode(StatusCodes.Status500InternalServerError, new Response()
        {
            Message = result.Reason,
            Success = false
        });
    }

    [HttpGet("Source")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContentSource([FromQuery] Guid videoId, [FromQuery] Guid sourceId, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!_cache.TryGetValue<CachedContentSource>($"{videoId}:{sourceId}", out var source))
        {
            source = await _contentRepository.GetContentSource(videoId, sourceId, cancellationToken);
            if (source is null) return NotFound();
            _cache.Set<CachedContentSource>($"{videoId}:{sourceId}", source, CacheOptions[source.Type]);
        }

        var response = File(source!.Data, source.ContentType, enableRangeProcessing: true);
        
        if (source!.Type == ContentSourceType.Video) return response;
        
        Response.Headers["Cache-Control"] = "public, max-age=3600";
        Response.Headers["Expires"] = DateTime.UtcNow.AddHours(1).ToString("R");

        return response;
    }
}