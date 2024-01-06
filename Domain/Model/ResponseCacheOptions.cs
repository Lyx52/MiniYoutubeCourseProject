using Microsoft.Extensions.Caching.Memory;

namespace Domain.Model;

public class ResponseCacheOptions
{
    private Dictionary<string, MemoryCacheEntryOptions> _options = new Dictionary<string, MemoryCacheEntryOptions>();

    public MemoryCacheEntryOptions this[string prefix] => _options.GetValueOrDefault(prefix, DefaultOptions);

    public MemoryCacheEntryOptions DefaultOptions { get; set; } = new MemoryCacheEntryOptions()
    {
        SlidingExpiration = TimeSpan.FromSeconds(15)
    };

    public ResponseCacheOptions AddEntryCache(string prefix, MemoryCacheEntryOptions options)
    {
        _options.Add(prefix, options);
        return this;
    }
}