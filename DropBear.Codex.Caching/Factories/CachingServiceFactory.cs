using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.Caching.CachingStrategies;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Caching.Factories;

/// <summary>
///     Factory for creating caching service instances based on specified cache types.
///     Supports optional encryption based on configuration settings.
/// </summary>
public class CachingServiceFactory(
    IEasyCachingProviderFactory providerFactory,
    CachingOptions cachingOptions,
    IServiceProvider serviceProvider)
    : ICachingServiceFactory, IDisposable
{
    private readonly CachingOptions _cachingOptions = cachingOptions ?? throw new ArgumentNullException(nameof(cachingOptions));
    private readonly IEasyCachingProviderFactory _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly List<ICacheService> _trackedServices = [];

    public ICacheService GetCachingService(CacheType cacheType)
    {
        var inMemoryLogger = _serviceProvider.GetRequiredService<IAppLogger<InMemoryCachingService>>();
        var fasterKvLogger = _serviceProvider.GetRequiredService<IAppLogger<FasterKvCachingService>>();
        var sqliteLogger = _serviceProvider.GetRequiredService<IAppLogger<SqLiteCachingService>>();

        ICacheService baseService = cacheType switch
        {
            CacheType.InMemory => new InMemoryCachingService(_providerFactory, _cachingOptions, inMemoryLogger),
            CacheType.FasterKv => new FasterKvCachingService(_providerFactory, _cachingOptions, fasterKvLogger),
            CacheType.SqLite => new SqLiteCachingService(_providerFactory, _cachingOptions, sqliteLogger),
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), $"Unsupported cache type: {cacheType}.")
        };

        // Track created service for disposal
        _trackedServices.Add(baseService);

        if (!_cachingOptions.EncryptionOptions.Enabled) return baseService;
        var encryptionLogger = _serviceProvider.GetRequiredService<IAppLogger<EncryptedCacheService>>();
        var dataProtectionProvider = _serviceProvider.GetRequiredService<IDataProtectionProvider>();

        var encryptedService =
            new EncryptedCacheService(baseService, dataProtectionProvider, _cachingOptions, encryptionLogger);
        _trackedServices.Add(encryptedService);

        return encryptedService;
    }

    public void Dispose()
    {
        foreach (var service in _trackedServices)
            if (service is IDisposable disposableService)
                disposableService.Dispose();
        _trackedServices.Clear();
    }
}
