using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core;
using Microsoft.Extensions.Options;

namespace DropBear.Codex.Caching.CachingStrategies;

/// <summary>
///     Provides a SQLite caching service utilizing EasyCaching.
/// </summary>
public class SQLiteCachingService : ICacheService, IDisposable
{
    private readonly IEasyCachingProvider _cache;
    private readonly CachingOptions _cacheOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SQLiteCachingService" /> class.
    /// </summary>
    /// <param name="factory">The EasyCaching provider factory.</param>
    /// <param name="cachingOptions"></param>
    public SQLiteCachingService(IEasyCachingProviderFactory factory, IOptions<CachingOptions> cachingOptions)
    {
        _cacheOptions = cachingOptions.Value;
        _cache = factory.GetCachingProvider(_cacheOptions.SQLiteOptions.CacheName);
    }


    /// <summary>
    ///     Retrieves a cached item by its key.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached item if found; otherwise, default.</returns>
    public async Task<T?> GetAsync<T>(string key)
    {
        var result = await _cache.GetAsync<T>(key);
        return result.HasValue ? result.Value : default;
    }

    /// <summary>
    ///     Adds an item to the cache with an optional expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The item to cache.</param>
    /// <param name="expiry">The expiration time.</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await _cache.SetAsync(key, value, expiry ?? _cacheOptions.DefaultCacheDurationMinutes);
    }

    /// <summary>
    ///     Removes a cached item by its key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    /// <summary>
    ///     Clears all items from the cache.
    /// </summary>
    public async Task FlushAsync()
    {
        await _cache.FlushAsync();
    }

    /// <summary>
    ///     Disposes the cache provider if it implements IDisposable.
    /// </summary>
    public void Dispose()
    {
        if (_cache is IDisposable disposableCache) disposableCache.Dispose();
    }
}