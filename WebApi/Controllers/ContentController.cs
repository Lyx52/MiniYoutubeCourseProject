using System.ComponentModel.DataAnnotations;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
    public ContentController(ILogger<ContentController> logger, IContentService contentService)
    {
        _logger = logger;
        _contentService = contentService;
    }

    [HttpPost("UploadVideoFile")]
    [FileExtensionValidation(MP4, MP4, WEBM)]
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
}