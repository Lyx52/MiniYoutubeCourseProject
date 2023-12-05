using System.Threading.Channels;
using Domain.Model;
using Domain.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services.Models;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VideoController : ControllerBase
{
    private readonly ILogger<VideoController> _logger;
    private readonly ChannelWriter<ProcessVideoTask> _channel;
    public VideoController(ILogger<VideoController> logger, ChannelWriter<ProcessVideoTask> channel)
    {
        _logger = logger;
        _channel = channel;
    }
    
    [HttpPost("CreateVideo")]
    public async Task<IActionResult> CreateVideo([FromBody] CreateVideoRequest payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _channel.WriteAsync(new ProcessVideoTask()
        {
            VideoId = Guid.NewGuid(),
            WorkSpaceId = payload.WorkSpaceId
        }, cancellationToken);
        return Ok();
    }
}