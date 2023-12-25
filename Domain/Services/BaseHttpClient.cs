using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class BaseHttpClient(
    string clientName,
    ILogger<dynamic> logger,
    IHttpClientFactory _httpClientFactory,
    ILoginManager? loginManager)
{
    private string ClientName { get; init; } = clientName;
    
    public async Task<TResponse> SendPayloadRequest<TPayload, TResponse>(
        string url,
        TPayload payload,
        JwtRequirement jwtRequirement = JwtRequirement.Optional,
        CancellationToken cancellationToken = default(CancellationToken)
    ) where TResponse : Response, new() {
        using var client = _httpClientFactory.CreateClient(ClientName);
        if (jwtRequirement is not JwtRequirement.None && loginManager is not null)
        {
            var jwt = await loginManager.GetJwtToken(cancellationToken);
            
            if (!string.IsNullOrEmpty(jwt))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");    
            }
            else if (jwtRequirement is JwtRequirement.Mandatory)
            {
                return new TResponse() { 
                    Success = false, 
                    Message = "Unauthorized request" 
                };
            }
        }

        try
        {
            var response = await client.PostAsJsonAsync<TPayload>(url, payload, cancellationToken);
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            logger.LogError("Request failed with exception {ExceptionMessage}!", e.Message);
            return new TResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"    
            };
        }
    }

    public Task<TResponse> SendQueryRequest<TResponse>(
        HttpMethod method,
        string url,
        JwtRequirement jwtRequirement = JwtRequirement.Optional,
        CancellationToken cancellationToken = default(CancellationToken)
    ) where TResponse : Response, new()
    {
        return SendQueryRequest<TResponse>(method, url, QueryString.Empty, jwtRequirement, cancellationToken);
    }

    public async Task<TResponse> SendQueryRequest<TResponse>(
        HttpMethod method,
        string url,
        QueryString query,
        JwtRequirement jwtRequirement = JwtRequirement.Optional,
        CancellationToken cancellationToken = default(CancellationToken)
    ) where TResponse : Response, new() {
        using var client = _httpClientFactory.CreateClient(ClientName);
        if (jwtRequirement is not JwtRequirement.None && loginManager is not null)
        {
            var jwt = await loginManager.GetJwtToken(cancellationToken);
            
            if (!string.IsNullOrEmpty(jwt))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");    
            }
            else if (jwtRequirement is JwtRequirement.Mandatory)
            {
                return new TResponse() { 
                    Success = false, 
                    Message = "Unauthorized request" 
                };
            }
        }

        try
        {
            var response = method.Method switch
            {
                "GET" => await client.GetAsync(url + query, cancellationToken),
                "DELETE" => await client.DeleteAsync(url + query, cancellationToken),
                _ => throw new NotSupportedException($"HttpMethod {method} not supported!")
            };
            return await HandleResponse<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            logger.LogError("Request failed with exception {ExceptionMessage}!", e.Message);
            return new TResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"    
            };
        }
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
        catch (JsonException e)
        {
            logger.LogError("Response parsing failed with exception {ExceptionMessage}", e.Message);
            return new TResponse()
            {
                Success = false,
                Message = "Request failed, please try again later"    
            };
        }
    }
}