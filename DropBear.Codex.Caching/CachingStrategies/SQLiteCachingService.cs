using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core;
using MethodTimer;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Caching.CachingStrategies;

/// <summary>
///     Provides a SQLite caching service utilizing EasyCaching.
/// </summary>
public class SQLiteCachingService : ICacheService, IDisposable
{
    private readonly IEasyCachingProvider _cache;
    private readonly CachingOptions _cacheOptions;
    private readonly ILogger<SQLiteCachingService> _logger;

    /// <summary>
    ///     Initializes a new instance of the SQLiteCachingService class.
    /// </summary>
    /// <param name="factory">The EasyCaching provider factory.</param>
    /// <param name="cachingOptions">The caching configuration options.</param>
    /// <param name="logger">The logger for capturing logs within the service.</param>
    public SQLiteCachingService(IEasyCachingProviderFactory factory, CachingOptions cachingOptions,
        ILogger<SQLiteCachingService>? logger)
    {
        _cacheOptions = cachingOptions ?? throw new ArgumentNullException(nameof(cachingOptions));
        _cache = factory.GetCachingProvider(_cacheOptions.SQLiteOptions.CacheName) ??
                 throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("SQLiteCachingService initialized.");
    }

    /// <summary>
    ///     Retrieves a cached item by its key. Uses a fallback function if the item is not found or on error.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="fallbackFunction">An optional function to retrieve the item if it's not found in the cache.</param>
    /// <returns>The cached item if found; otherwise, the result of the fallback function, or default.</returns>
    [Time]
    public async Task<T?> GetAsync<T>(string key, Func<Task<T?>>? fallbackFunction = null)
    {
        try
        {
            var result = await _cache.GetAsync<T>(key);
            if (result.HasValue) return result.Value;

            if (fallbackFunction == null) return default;
            _logger.ZLogInformation($"Cache miss for key {key}. Attempting to retrieve from fallback function.");
            return await fallbackFunction();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error retrieving cache item for key: {key}. Attempting fallback if available.");
            if (fallbackFunction == null) return default; // Return default if the fallback is not provided or fails.
            try
            {
                return await fallbackFunction();
            }
            catch (Exception fallbackEx)
            {
                _logger.ZLogError(fallbackEx, $"Fallback function also failed for key: {key}.");
            }

            return default; // Return default if the fallback is not provided or fails.
        }
    }


    /// <summary>
    ///     Adds an item to the cache with an optional expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The item to cache.</param>
    /// <param name="expiry">The expiration time.</param>
    [Time]
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            await _cache.SetAsync(key, value,
                expiry ?? _cacheOptions.DefaultCacheDurationMinutes);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error setting cache item for key: {key}");
        }
    }

    /// <summary>
    ///     Removes a cached item by its key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    [Time]
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error removing cache item for key: {key}");
        }
    }

    /// <summary>
    ///     Clears all items from the cache.
    /// </summary>
    [Time]
    public async Task FlushAsync()
    {
        try
        {
            await _cache.FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error flushing cache.");
        }
    }

    /// <summary>
    ///     Disposes the cache provider if it implements IDisposable.
    /// </summary>
    public void Dispose()
    {
        if (_cache is IDisposable disposableCache) disposableCache.Dispose();
    }
}