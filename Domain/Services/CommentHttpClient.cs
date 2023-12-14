using System.Net;
using System.Net.Http.Json;
using Domain.Interfaces;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class CommentHttpClient : ICommentHttpClient
{
    private readonly ILogger<CommentHttpClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoginManager _loginManager;
    public CommentHttpClient(ILogger<CommentHttpClient> logger, IHttpClientFactory httpClientFactory, ILoginManager loginManager)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _loginManager = loginManager;
    }
    
    public async Task<QueryCommentsResponse> GetVideoComments(string videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var client = _httpClientFactory.CreateClient(nameof(CommentHttpClient));
        try
        {
            var response = await client.GetFromJsonAsync<QueryCommentsResponse>($"api/Comment/Query?id={videoId}",
                cancellationToken);
            return response ?? new QueryCommentsResponse()
            {
                Success = false,
                Message = "Request failed, please try again later",
                Comments = new List<CommentModel>()
            };
        }
        catch (Exception e)
        {
            _logger.LogError("CommentApi request failed {ExceptionMessage}!", e.Message);
            return new QueryCommentsResponse()
            {
                Success = false,
                Message = "Request failed, please try again later",
                Comments = new List<CommentModel>()
            };
        }
    }

    public async Task<Response> CreateComment(Guid videoId, string message, CancellationToken cancellationToken = default(CancellationToken))
    {
        var jwt = await _loginManager.GetJwtToken(cancellationToken);
        if (jwt is null)
        {
            return new Response()
            {
                Message = "Unauthorized",
                Success = false
            };
        }
        
        using var client = _httpClientFactory.CreateClient(nameof(CommentHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        HttpResponseMessage? postResponse;
        try
        {
            postResponse = await client.PostAsJsonAsync<CreateCommentRequest>("api/Comment/Create", new CreateCommentRequest()
            {
                VideoId = videoId,
                Message = message
            }, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("CommentApi request failed {ExceptionMessage}!", e.Message);
            return new Response()
            {
                Message = "Request failed, please try again later",
                Success = false
            };
        }
        var statusMessage = postResponse.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Created => string.Empty,
            _ => "Request failed, please try again later"
        };
        return new Response()
        {
            Message = statusMessage,
            Success = true
        };
    }

    public async Task AddLikeDislike(string commentId, bool isLike, CancellationToken cancellationToken = default(CancellationToken))
    {
        var jwt = await _loginManager.GetJwtToken(cancellationToken);
        if (jwt is null) return;
        using var client = _httpClientFactory.CreateClient(nameof(CommentHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        try
        {
            await client.PostAsJsonAsync<LikeDislikeRequest>("api/Comment/LikeDislike", new LikeDislikeRequest()
            {
                CommentId = commentId,
                IsLike = isLike
            }, cancellationToken);
        } catch (HttpRequestException e)
        {
            _logger.LogError("CommentApi request failed {ExceptionMessage}!", e.Message);
            return;
        }
    }
}