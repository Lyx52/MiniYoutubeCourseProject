using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly ILogger<ContentController> _logger;
    public ContentController(ILogger<ContentController> logger)
    {
        _logger = logger;
    }

    [HttpPost("UploadVideoFile")]
    public async Task<IActionResult> UploadVideoFile([FromForm] IFormFile file)
    {
        return Ok();
    }
}