using System.Net;
using System.Net.Http.Json;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class AuthHttpClient : IAuthHttpClient
{
    private readonly ILogger<AuthHttpClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    public AuthHttpClient(ILogger<AuthHttpClient> logger, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<LoginResponse> LoginAsync(LoginModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var client = _httpClientFactory.CreateClient(nameof(AuthHttpClient));
        HttpResponseMessage? postResponse;
        try
        {
            postResponse = await client.PostAsJsonAsync<LoginRequest>("api/Auth/Login", new LoginRequest()
            {
                Password = model.Password,
                Username = model.Username
            }, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("AuthApi request failed {ExceptionMessage}!", e.Message);
            return new LoginResponse()
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
        var content = await postResponse.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        if (content is not null)
        {
            content.Message = message;
            return content;
        }
        return new LoginResponse()
        {
            Expiration = DateTime.MaxValue,
            Message = message,
            Success = false,
            Token = string.Empty,
            BearerToken = string.Empty
        };
    }

    public async Task<UserProfileResponse> GetUserProfile(string jwt, CancellationToken cancellationToken = default(CancellationToken))
    {
        using var client = _httpClientFactory.CreateClient(nameof(AuthHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        try
        {
            var content = await client.GetFromJsonAsync<UserProfileResponse>("api/Auth/Profile", cancellationToken);
            if (content is not null) return content;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("AuthApi request failed {ExceptionMessage}!", e.Message);
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
    public async Task<Response> RegisterAsync(RegisterModel model)
    {
        using HttpClient client = _httpClientFactory.CreateClient(nameof(AuthHttpClient));
        HttpResponseMessage? postResponse;
        try {
            postResponse = await client.PostAsJsonAsync<RegisterRequest>("api/Auth/Register", new RegisterRequest()
            {
                Password = model.Password,
                Username = model.Username,
                Email = model.Email
            });
            if (postResponse.IsSuccessStatusCode)
            {
                return new Response()
                {
                    Success = true
                };
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("AuthApi request failed {ExceptionMessage}!", e.Message);
            return new LoginResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"
            };
        }
        
        var content = await postResponse.Content.ReadFromJsonAsync<Response>();
        if (content is not null) return content;
        return new Response()
        {
            Message = "Request failed, please try again later",
            Success = false,
        };
    }
}