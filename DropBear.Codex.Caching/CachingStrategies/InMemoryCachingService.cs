using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core;

namespace DropBear.Codex.Caching.CachingStrategies;

/// <summary>
///     Provides an in-memory caching service utilizing EasyCaching.
/// </summary>
public class InMemoryCachingService : ICacheService, IDisposable, IAsyncDisposable
{
    private readonly IEasyCachingProvider _cache;
    private readonly CachingOptions _cacheOptions;
    private readonly IAppLogger<InMemoryCachingService> _logger;

    /// <summary>
    ///     Initializes a new instance of the InMemoryCachingService class.
    /// </summary>
    /// <param name="factory">The EasyCaching provider factory.</param>
    /// <param name="cachingOptions">The caching configuration options.</param>
    /// <param name="logger">The logger instance for logging operations and errors.</param>
    public InMemoryCachingService(IEasyCachingProviderFactory factory, CachingOptions cachingOptions,
        IAppLogger<InMemoryCachingService>? logger)
    {
        _cache = factory.GetCachingProvider(cachingOptions.InMemoryOptions.CacheName) ??
                 throw new ArgumentNullException(nameof(factory));
        _cacheOptions = cachingOptions ?? throw new ArgumentNullException(nameof(cachingOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("InMemoryCachingService initialized.");
    }

    public async ValueTask DisposeAsync()
    {
        // Asynchronously dispose of any resources
        await DisposeAsyncCore().ConfigureAwait(false);

        // Dispose of any synchronous resources as well
        Dispose(false); // Pass false to indicate asynchronous disposal

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Retrieves a cached item by its key. Uses a fallback function if the item is not found or on error.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="fallbackFunction">An optional function to retrieve the item if it's not found in the cache.</param>
    /// <returns>The cached item if found; otherwise, the result of the fallback function, or default.</returns>
    public async Task<T?> GetAsync<T>(string key, Func<Task<T?>>? fallbackFunction = null)
    {
        try
        {
            var result = await _cache.GetAsync<T>(key).ConfigureAwait(false);
            if (result.HasValue) return result.Value;

            if (fallbackFunction is null) return default;
            _logger.LogInformation(
                ZString.Format($"Cache miss for key {key}. Attempting to retrieve from fallback function.", key));
            return await fallbackFunction().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ZString.Format($"Error retrieving cache item for key: {key}", key));
            if (fallbackFunction is null) return default; // Return default if the fallback is not provided or fails.
            try
            {
                return await fallbackFunction().ConfigureAwait(false);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, ZString.Format($"Error executing fallback function for key: {key}", key));
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
    /// <param name="expiry">The expiration time, if any.</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            await _cache.SetAsync(key, value,
                expiry ?? _cacheOptions.DefaultCacheDurationMinutes).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ZString.Format($"Error setting cache item for key: {key}", key));
        }
    }

    /// <summary>
    ///     Removes a cached item by its key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ZString.Format($"Error removing cache item for key: {key}", key));
        }
    }

    /// <summary>
    ///     Clears all items from the cache.
    /// </summary>
    public async Task FlushAsync()
    {
        try
        {
            await _cache.FlushAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing cache.");
        }
    }

    /// <summary>
    ///     Disposes the cache provider if it implements IDisposable.
    /// </summary>
    public void Dispose()
    {
        // Synchronously dispose of any resources
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose synchronous resources here
        }
    }

    private async ValueTask DisposeAsyncCore()
    {
        switch (_cache)
        {
            // Asynchronously dispose of any resources here
            // ReSharper disable once SuspiciousTypeConversion.Global
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}
