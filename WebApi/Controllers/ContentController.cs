using System.ComponentModel.DataAnnotations;
using Domain.Constants;
using Domain.Entity;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
    //
    // [FileExtensionValidation(MP4, MP4, WEBM)]
    public async Task<IActionResult> UploadVideoFile([FromForm] IFormFile videoFile, CancellationToken cancellationToken = default(CancellationToken))
    {
        MemoryStream memoryStream = new MemoryStream();
        await videoFile.OpenReadStream().CopyToAsync(memoryStream, cancellationToken);
        var id = await _contentService.SaveTemporaryFile(memoryStream, videoFile.FileName, cancellationToken);
        if (id.HasValue)
        {
                
            return Ok(new UploadVideoFileResponse()
            {
                FileId = id.Value,
                FileName = $"{id}{Path.GetExtension(videoFile.FileName)}",
                Success = true,
            });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new Response()
        {
            Message = "Failed to save video file as temporary file",
            Success = false
        });
    }

    [HttpGet("Source")]
    [AllowAnonymous]
    public async Task<IActionResult> GetContentSource([FromQuery] string videoId, string sourceId, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (Guid.TryParse(videoId, out var vid) && Guid.TryParse(sourceId, out var sid))
        {
            if (!_cache.TryGetValue<CachedContentSource>($"{videoId}:{sourceId}", out var source))
            {
                source = await _contentRepository.GetContentSource(vid, sid, cancellationToken);
                if (source is null) return NotFound();
                _cache.Set<CachedContentSource>($"{videoId}:{sourceId}", source, CacheOptions[source.Type]);
            }
            return File(source!.Data, source.ContentType, enableRangeProcessing: true);
        }

        return BadRequest("Invalid video id or source id");
    }
}