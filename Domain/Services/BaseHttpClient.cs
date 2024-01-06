using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public abstract class BaseHttpClient
{
    private readonly ILogger<dynamic> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoginManager? _loginManager;
    public BaseHttpClient(string clientName,
        ILogger<dynamic> logger,
        IHttpClientFactory httpClientFactory,
        ILoginManager? loginManager)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _loginManager = loginManager;
        ClientName = clientName;
    }

    private string ClientName { get; init; }
    
    public async Task<TResponse> SendPayloadRequest<TPayload, TResponse>(
        string url,
        TPayload payload,
        string? jwtToken = null,
        CancellationToken cancellationToken = default(CancellationToken)
    ) where TResponse : Response, new() {
        using var client = _httpClientFactory.CreateClient(ClientName);
        if (!string.IsNullOrEmpty(jwtToken))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
        }

        try
        {
            var response = await client.PostAsJsonAsync<TPayload>(url, payload, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized && _loginManager is not null && url != "api/Auth/RefreshToken")
            {
                var refreshResponse = await RefreshTokenInternalAsync(cancellationToken);
                if (refreshResponse.Success)
                {
                    // Try to resend using new token
                    await _loginManager.SetRefreshToken(refreshResponse.RefreshToken, cancellationToken);
                    await _loginManager.SetJwtToken(refreshResponse.Token, cancellationToken);
                    var newRequestResponse = await SendPayloadRequest<TPayload, TResponse>(url, payload, refreshResponse.Token, cancellationToken);
                    if (newRequestResponse.Success) return newRequestResponse;
                }
            }
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Request failed with exception {ExceptionMessage}!", e.Message);
            return new TResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"    
            };
        }
    }
    
    public async Task<TResponse> SendPayloadRequest<TPayload, TResponse>(
        string url,
        TPayload payload,
        JwtRequirement jwtRequirement = JwtRequirement.Optional,
        CancellationToken cancellationToken = default(CancellationToken)
    ) where TResponse : Response, new()
    {
        string? jwtToken = null;
        if (jwtRequirement is not JwtRequirement.None && _loginManager is not null)
        {
            jwtToken = await _loginManager.GetJwtToken(cancellationToken);
            
            if (string.IsNullOrEmpty(jwtToken) && jwtRequirement is JwtRequirement.Mandatory)
            {
                return new TResponse() { 
                    Success = false, 
                    Message = "Unauthorized request" 
                };
            }
        }

        return await SendPayloadRequest<TPayload, TResponse>(url, payload, jwtToken, cancellationToken);
    }
    public async Task<TResponse> SendQueryRequest<TResponse>(
        HttpMethod method,
        string url,
        QueryString query,
        string? jwtToken = null,
        CancellationToken cancellationToken = default(CancellationToken)
    ) where TResponse : Response, new() {
        using var client = _httpClientFactory.CreateClient(ClientName);
        if (!string.IsNullOrEmpty(jwtToken))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
        }

        try
        {
            var response = method.Method switch
            {
                "GET" => await client.GetAsync(url + query, cancellationToken),
                "DELETE" => await client.DeleteAsync(url + query, cancellationToken),
                _ => throw new NotSupportedException($"HttpMethod {method} not supported!")
            };
            if (response.StatusCode == HttpStatusCode.Unauthorized && _loginManager is not null && url != "api/Auth/RefreshToken")
            {
                var refreshResponse = await RefreshTokenInternalAsync(cancellationToken);
                if (refreshResponse.Success)
                {
                    // Try to resend using new token
                    await _loginManager.SetRefreshToken(refreshResponse.RefreshToken, cancellationToken);
                    await _loginManager.SetJwtToken(refreshResponse.Token, cancellationToken);
                    var newRequestResponse = await SendQueryRequest<TResponse>(method, url, query, refreshResponse.Token, cancellationToken);
                    if (newRequestResponse.Success) return newRequestResponse;
                }
            }
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Request failed with exception {ExceptionMessage}!", e.Message);
            return new TResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"    
            };
        }
    }

    public async Task<TResponse> SendQueryRequest<TResponse>(
        HttpMethod method,
        string url,
        QueryString query,
        JwtRequirement jwtRequirement = JwtRequirement.Optional,
        CancellationToken cancellationToken = default(CancellationToken)
    ) where TResponse : Response, new() {
        string? jwtToken = null;
        if (jwtRequirement is not JwtRequirement.None && _loginManager is not null)
        {
            jwtToken = await _loginManager.GetJwtToken(cancellationToken);
            
            if (string.IsNullOrEmpty(jwtToken) && jwtRequirement is JwtRequirement.Mandatory)
            {
                return new TResponse() { 
                    Success = false, 
                    Message = "Unauthorized request" 
                };
            }
        }

        return await SendQueryRequest<TResponse>(method, url, query, jwtToken, cancellationToken);
    }
    private async Task<TResponse> HandleResponse<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken = default(CancellationToken)) 
        where TResponse : Response, new()
    {
        try
        {
            var content = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
            return content ?? new TResponse()
            {
                Success = response.IsSuccessStatusCode,
                Message = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Unauthorized",
                    HttpStatusCode.OK => string.Empty,
                    _ => "Request failed, please try again later"
                }
            };   
        }
        catch (Exception e)
        {
            _logger.LogError("Response parsing failed with exception {ExceptionMessage}", e.Message);
            return new TResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"    
            };
        }
    }

    protected Task<RefreshTokenResponse> RefreshTokenInternalAsync(CancellationToken cancellationToken = default(CancellationToken))
        => RefreshTokenInternalAsync(_loginManager!, cancellationToken);

    protected async Task<RefreshTokenResponse> RefreshTokenInternalAsync(ILoginManager loginManager, 
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var refreshToken = await loginManager.GetRefreshToken(cancellationToken);
        var expiredToken = await loginManager.GetJwtToken(cancellationToken);
        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(expiredToken))
        {
            return new RefreshTokenResponse()
            {
                Success = false,
                Message = "Unauthorized, please try authenticating again"
            };
        }
        
        return await SendPayloadRequest<RefreshTokenRequest, RefreshTokenResponse>("api/Auth/RefreshToken", new RefreshTokenRequest()
        {
            ExpiredToken = expiredToken,
            RefreshToken = refreshToken
        }, JwtRequirement.None, cancellationToken);
    }
}