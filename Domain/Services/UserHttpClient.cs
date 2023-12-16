using System.Net;
using System.Net.Http.Json;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class UserHttpClient : IUserHttpClient
{
    private readonly ILogger<UserHttpClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoginManager _loginManager;
    public UserHttpClient(
        ILogger<UserHttpClient> logger, 
        IHttpClientFactory httpClientFactory, 
        ILoginManager loginManager)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _loginManager = loginManager;
    }
    
    public async Task<UserProfileResponse> GetPublicUserProfile(Guid userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var client = _httpClientFactory.CreateClient(nameof(UserHttpClient));
        try
        {
            var content = await client.GetFromJsonAsync<UserProfileResponse>($"api/User/PublicProfile?userId={userId.ToString()}", cancellationToken);
            if (content is not null) return content;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("UserApi request failed {ExceptionMessage}!", e.Message);
            return new UserProfileResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        return new UserProfileResponse()
        {
            Success = false,
            Message = "Request failed, please try again later"
        };
    }
    
    public async Task<UserProfileResponse> GetUserProfile(CancellationToken cancellationToken = default(CancellationToken))
    {
        var jwt = await _loginManager.GetJwtToken(cancellationToken);
        if (jwt is null)
        {
            return new UserProfileResponse()
            {
                Message = "Unauthorized",
                Success = false
            };
        }
        
        using var client = _httpClientFactory.CreateClient(nameof(UserHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        try
        {
            var content = await client.GetFromJsonAsync<UserProfileResponse>("api/User/Profile", cancellationToken);
            if (content is not null) return content;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("UserApi request failed {ExceptionMessage}!", e.Message);
            return new UserProfileResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        return new UserProfileResponse()
        {
            Success = false,
            Message = "Request failed, please try again later"
        };
    }
    
    public async Task<Response> Subscribe(string creatorId, CancellationToken cancellationToken = default(CancellationToken))
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
        
        using var client = _httpClientFactory.CreateClient(nameof(UserHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        HttpResponseMessage? response;
        try
        {
            response = await client.PostAsJsonAsync<SubscribeRequest>($"api/User/Subscribe?creatorId={creatorId}", new SubscribeRequest()
            {
                CreatorId = creatorId
            }, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("UserApi request failed {ExceptionMessage}!", e.Message);
            return new UserProfileResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        return new Response()
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode ? "Request failed, please try again later" : string.Empty
        };
    }
    public async Task<Response> Unsubscribe(string creatorId, CancellationToken cancellationToken = default(CancellationToken))
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
        
        using var client = _httpClientFactory.CreateClient(nameof(UserHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        HttpResponseMessage? response;
        try
        {
            response = await client.PostAsJsonAsync<UnsubscribeRequest>($"api/User/Unsubscribe?creatorId={creatorId}", new UnsubscribeRequest()
            {
                CreatorId = creatorId
            }, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("UserApi request failed {ExceptionMessage}!", e.Message);
            return new UserProfileResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        return new Response()
        {
            Success = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode ? "Request failed, please try again later" : string.Empty
        };
    }
}