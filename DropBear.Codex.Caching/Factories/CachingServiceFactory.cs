using DropBear.Codex.Caching.CachingStrategies;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.Caching.Factories;

/// <summary>
///     Factory for creating caching service instances based on specified cache types.
///     Supports optional encryption based on configuration settings.
/// </summary>
public class CachingServiceFactory : ICachingServiceFactory
{
    private readonly CachingOptions _cachingOptions;
    private readonly IEasyCachingProviderFactory _providerFactory;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CachingServiceFactory" /> class.
    /// </summary>
    /// <param name="providerFactory">The factory used to create caching providers.</param>
    /// <param name="cachingOptions">The options for configuring caching services.</param>
    /// <param name="serviceProvider">The service provider for resolving additional services like data protection.</param>
    public CachingServiceFactory(
        IEasyCachingProviderFactory providerFactory,
        CachingOptions cachingOptions,
        IServiceProvider serviceProvider)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _cachingOptions = cachingOptions ?? throw new ArgumentNullException(nameof(cachingOptions));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    ///     Creates and returns an <see cref="ICacheService" /> instance based on the specified <see cref="CacheType" />.
    ///     Wraps the cache service with encryption functionality if enabled in the caching options.
    /// </summary>
    /// <param name="cacheType">The type of cache to create.</param>
    /// <returns>
    ///     An instance of an object that implements <see cref="ICacheService" />, configured according to the cache type
    ///     and encryption settings.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported <see cref="CacheType" /> is specified.</exception>
    public ICacheService GetCachingService(CacheType cacheType)
    {
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        // Correctly create generic loggers for each caching service type
        var inmemoryLogger = loggerFactory.CreateLogger<InMemoryCachingService>();
        var fasterKVLogger = loggerFactory.CreateLogger<FasterKVCachingService>();
        var sqliteLogger = loggerFactory.CreateLogger<SQLiteCachingService>();


        ICacheService baseService = cacheType switch
        {
            CacheType.InMemory => new InMemoryCachingService(_providerFactory, _cachingOptions,
                inmemoryLogger),
            CacheType.FasterKV => new FasterKVCachingService(_providerFactory, _cachingOptions,
                fasterKVLogger),
            CacheType.SQLite => new SQLiteCachingService(_providerFactory, _cachingOptions,
                sqliteLogger),
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), $"Unsupported cache type: {cacheType}.")
        };

        if (_cachingOptions.EncryptionOptions.Enabled)
        {
            var encryptionLogger = loggerFactory.CreateLogger<EncryptedCacheService>();
            var dataProtectionProvider = _serviceProvider.GetRequiredService<IDataProtectionProvider>();
            // Correctly use IDataProtectionProvider without converting to IDataProtector
            return new EncryptedCacheService(baseService, dataProtectionProvider, _cachingOptions,
                encryptionLogger);
        }

        return baseService;
    }
}