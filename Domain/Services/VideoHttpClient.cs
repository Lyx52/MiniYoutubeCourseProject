using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Constants;
using Domain.Entity;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.Response;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class VideoHttpClient : IVideoHttpClient
{
    private readonly ILogger<VideoHttpClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoginManager _loginManager;

    public VideoHttpClient(
        ILogger<VideoHttpClient> logger, 
        IHttpClientFactory httpClientFactory,
        ILoginManager loginManager)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _loginManager = loginManager;
    }

    public async Task<CreateVideoResponse> CreateVideo(CreateVideoModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        var jwt = await _loginManager.GetJwtToken(cancellationToken);
        if (jwt is null)
        {
            return new CreateVideoResponse()
            {
                Message = "Unauthorized",
                Success = false
            };
        }
        
        using var client = _httpClientFactory.CreateClient(nameof(VideoHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        HttpResponseMessage? postResponse;
        try
        {
            postResponse = await client.PostAsJsonAsync<CreateVideoRequest>("api/Video/CreateVideo", new CreateVideoRequest()
            {
                Description = model.Description,
                Title = model.Title,
                WorkSpaceId = model.WorkSpaceId,
                IsUnlisted = model.IsUnlisted
            }, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("VideoApi request failed {ExceptionMessage}!", e.Message);
            return new CreateVideoResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        var message = postResponse.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Invalid username or password",
            HttpStatusCode.OK => string.Empty,
            _ => "Request failed, please try again later"
        };
        var content = await postResponse.Content.ReadFromJsonAsync<CreateVideoResponse>(cancellationToken);
        if (content is not null)
        {
            content.Message = message;
            return content;
        }
        return new CreateVideoResponse()
        {
            Message = message,
            Success = false,
        };
    }
    
    public async Task<Response> PublishVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var jwt = await _loginManager.GetJwtToken(cancellationToken);
        if (jwt is null)
        {
            return new CreateVideoResponse()
            {
                Message = "Unauthorized",
                Success = false
            };
        }
        
        using var client = _httpClientFactory.CreateClient(nameof(VideoHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        HttpResponseMessage? postResponse;
        try
        {
            postResponse = await client.PostAsJsonAsync<PublishVideoRequest>("api/Video/PublishVideo", new PublishVideoRequest()
            {
                VideoId = videoId
            }, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("VideoApi request failed {ExceptionMessage}!", e.Message);
            return new Response()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        var message = postResponse.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Invalid username or password",
            HttpStatusCode.OK => string.Empty,
            _ => "Request failed, please try again later"
        };
        return new Response()
        {
            Message = message,
            Success = postResponse.IsSuccessStatusCode,
        };
    }

    public async Task<VideoStatusResponse> GetProcessingStatus(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var jwt = await _loginManager.GetJwtToken(cancellationToken);
        if (jwt is null)
        {
            return new VideoStatusResponse()
            {
                Message = "Unauthorized",
                Success = false
            };
        }
        
        using var client = _httpClientFactory.CreateClient(nameof(VideoHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        HttpResponseMessage? postResponse;
        try
        {
            postResponse = await client.PostAsJsonAsync<VideoStatusRequest>("api/Video/Status", new VideoStatusRequest()
            {
                VideoId = videoId
            }, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("VideoApi request failed {ExceptionMessage}!", e.Message);
            return new VideoStatusResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        var message = postResponse.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Invalid username or password",
            HttpStatusCode.OK => string.Empty,
            _ => "Request failed, please try again later"
        };
        var content = await postResponse.Content.ReadFromJsonAsync<VideoStatusResponse>(cancellationToken);
        if (content is not null)
        {
            content.Message = message;
            return content;
        }
        return new VideoStatusResponse()
        {
            Message = message,
            Success = false,
        };
    }

    public async Task<SearchVideosResponse> GetVideosByTitle(string searchText, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var client = _httpClientFactory.CreateClient(nameof(VideoHttpClient));
        try
        {
            var response = await client.GetFromJsonAsync<SearchVideosResponse>($"api/Video/Query?searchText={searchText}",
                cancellationToken);
            return response ?? new SearchVideosResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        catch (Exception e)
        {
            _logger.LogError("VideoApi request failed {ExceptionMessage}!", e.Message);
            return new SearchVideosResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
    }
    
    public async Task<VideoMetadataResponse> GetVideoMetadata(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var client = _httpClientFactory.CreateClient(nameof(VideoHttpClient));
        try
        {
            var response = await client.GetFromJsonAsync<VideoMetadataResponse>($"api/Video/Metadata?id={videoId.ToString()}",
                cancellationToken);
            return response ?? new VideoMetadataResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        catch (Exception e)
        {
            _logger.LogError("VideoApi request failed {ExceptionMessage}!", e.Message);
            return new VideoMetadataResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
    }
}