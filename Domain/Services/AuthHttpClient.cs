using System.Net;
using System.Net.Http.Json;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class AuthHttpClient(
    ILogger<AuthHttpClient> logger,
    IHttpClientFactory httpClientFactory)
    : BaseHttpClient(nameof(AuthHttpClient), logger, httpClientFactory, null), IAuthHttpClient
{
    private readonly ILogger<AuthHttpClient> _logger = logger;

    public Task<LoginResponse> LoginAsync(LoginModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<LoginRequest, LoginResponse>("api/Auth/Login", new LoginRequest()
        {
            Password = model.Password,
            Username = model.Username
        }, JwtRequirement.None, cancellationToken);
    }
    
    public Task<Response> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<RegisterRequest, Response>("api/Auth/Register", new RegisterRequest()
        {
            Password = model.Password,
            Username = model.Username,
            Email = model.Email
        }, JwtRequirement.None, cancellationToken);
    }
}