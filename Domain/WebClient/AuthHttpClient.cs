using System.Net;
using System.Net.Http.Json;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.Extensions.Logging;

namespace Domain.WebClient;

public class AuthHttpClient : IAuthHttpClient
{
    private readonly ILogger<AuthHttpClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    public AuthHttpClient(ILogger<AuthHttpClient> logger, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<LoginResponseModel> LoginAsync(LoginModel model)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        var postResponse = await client.PostAsJsonAsync<LoginModel>("api/Auth/Login", model);
        if (!postResponse.IsSuccessStatusCode)
        {
            return new LoginResponseModel()
            {
                Expiration = DateTime.MaxValue,
                Message = $"Request failed with status: {postResponse.StatusCode}",
                Success = false,
                Token = string.Empty,
                BearerToken = string.Empty
            };
        }

        var content = await postResponse.Content.ReadFromJsonAsync<LoginResponseModel>();
        if (content is not null) return content;
        return new LoginResponseModel()
        {
            Expiration = DateTime.MaxValue,
            Message = "Failed to parse response from server",
            Success = false,
            Token = string.Empty,
            BearerToken = string.Empty
        };

    }

    public async Task<Response> RegisterAsync(RegisterModel model)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        var postResponse = await client.PostAsJsonAsync<RegisterModel>("api/Auth/Register", model);
        if (!postResponse.IsSuccessStatusCode)
        {
            return new Response()
            {
                Message = $"Request failed with status: {postResponse.StatusCode}",
                Success = false,
            };
        }

        var content = await postResponse.Content.ReadFromJsonAsync<Response>();
        if (content is not null) return content;
        return new Response()
        {
            Message = "Failed to parse response from server",
            Success = false,
        };
    }
}