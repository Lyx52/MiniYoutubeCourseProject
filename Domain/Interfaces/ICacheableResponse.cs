namespace Domain.Interfaces;

public interface IResponseCachingService
{
    Task<TResponse> AsCachedResponse<TResponse>(string prefix, string key, Func<Task<TResponse>> requestFunction);
}