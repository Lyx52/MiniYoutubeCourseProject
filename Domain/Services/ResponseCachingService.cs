using Domain.Interfaces;
using Domain.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Domain.Services;

public class ResponseCachingService : IResponseCachingService
{
    private readonly IMemoryCache _cache;
    private ResponseCacheOptions _options;

    public ResponseCachingService(ILogger<ResponseCachingService> logger, IMemoryCache cache, IOptions<ResponseCacheOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }
    public async Task<TResponse> AsCachedResponse<TResponse>(string prefix, string key, Func<Task<TResponse>> requestFunction)
    {
        if (_cache.TryGetValue<TResponse>($"{prefix}{key}", out var response) && response is not null) return response;
        response = await requestFunction();
        _cache.Set<TResponse>($"{prefix}{key}", response, _options[prefix]);
        return response;
    }
}