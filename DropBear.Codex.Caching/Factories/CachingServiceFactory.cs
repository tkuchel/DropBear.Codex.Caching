using DropBear.Codex.Caching.CachingStrategies;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core;
using Microsoft.Extensions.Options;

namespace DropBear.Codex.Caching.Factories;

/// <summary>
/// Factory for creating caching service instances based on the specified cache type.
/// </summary>
public class CachingServiceFactory : ICachingServiceFactory
{
    private readonly IEasyCachingProviderFactory _providerFactory;
    private readonly IOptions<CachingOptions> _cacheOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingServiceFactory"/> class.
    /// </summary>
    /// <param name="providerFactory">The EasyCaching provider factory used to create caching providers.</param>
    /// <param name="cachingOptions">The options for configuring caching services.</param>
    public CachingServiceFactory(IEasyCachingProviderFactory providerFactory, IOptions<CachingOptions> cachingOptions)
    {
        _providerFactory = providerFactory;
        _cacheOptions = cachingOptions;
    }

    /// <summary>
    /// Creates and returns an <see cref="ICacheService"/> instance based on the specified <see cref="CacheType"/>.
    /// </summary>
    /// <param name="cacheType">The type of cache to create.</param>
    /// <returns>An instance of an object that implements <see cref="ICacheService"/>, configured according to the cache type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported <see cref="CacheType"/> is specified.</exception>
    public ICacheService GetCachingService(CacheType cacheType)
    {
        return cacheType switch
        {
            CacheType.InMemory => new InMemoryCachingService(_providerFactory, _cacheOptions),
            CacheType.FasterKV => new FasterKVCachingService(_providerFactory, _cacheOptions),
            CacheType.SQLite => new SQLiteCachingService(_providerFactory, _cacheOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), $"Unsupported cache type: {cacheType}.")
        };
    }
}
