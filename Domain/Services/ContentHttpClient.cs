using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Interfaces;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class ContentHttpClient : IContentHttpClient
{
    private readonly ILogger<ContentHttpClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoginManager _loginManager;
    public ContentHttpClient(ILogger<ContentHttpClient> logger, IHttpClientFactory httpClientFactory, ILoginManager loginManager)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _loginManager = loginManager;
    }

    public async Task<UploadVideoFileResponse> UploadVideoFile(UploadVideoModel model,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var jwt = await _loginManager.GetJwtToken(cancellationToken);
        if (jwt is null)
        {
            return new UploadVideoFileResponse()
            {
                Message = "Unauthorized",
                Success = false
            };
        }

        using var client = _httpClientFactory.CreateClient(nameof(ContentHttpClient));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        model.FileStream.Seek(0, SeekOrigin.Begin);
        HttpResponseMessage? postResponse;
        try
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StreamContent(model.FileStream), "videoFile", model.FileName);
            postResponse = await client.PostAsync("api/Content/UploadVideoFile", formData, cancellationToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("AuthApi request failed {ExceptionMessage}!", e.Message);
            return new UploadVideoFileResponse()
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
        var content = await postResponse.Content.ReadFromJsonAsync<UploadVideoFileResponse>(cancellationToken);
        if (content is not null)
        {
            content.Message = message;
            return content;
        }

        return new UploadVideoFileResponse()
        {
            Success = false,
            Message = message,
        };
    }
}