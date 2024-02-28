using DropBear.Codex.Caching.CachingStrategies;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace DropBear.Codex.Caching.Factories;

/// <summary>
///     Factory for creating caching service instances based on specified cache types.
///     Supports optional encryption based on configuration settings.
/// </summary>
public class CachingServiceFactory : ICachingServiceFactory, IDisposable
{
    private readonly CachingOptions _cachingOptions;
    private readonly IEasyCachingProviderFactory _providerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ICacheService> _trackedServices = new List<ICacheService>();

    public CachingServiceFactory(
        IEasyCachingProviderFactory providerFactory,
        CachingOptions cachingOptions,
        IServiceProvider serviceProvider)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _cachingOptions = cachingOptions ?? throw new ArgumentNullException(nameof(cachingOptions));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public ICacheService GetCachingService(CacheType cacheType)
    {
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        var inMemoryLogger = loggerFactory.CreateLogger<InMemoryCachingService>();
        var fasterKvLogger = loggerFactory.CreateLogger<FasterKvCachingService>();
        var sqliteLogger = loggerFactory.CreateLogger<SqLiteCachingService>();

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
        var encryptionLogger = loggerFactory.CreateLogger<EncryptedCacheService>();
        var dataProtectionProvider = _serviceProvider.GetRequiredService<IDataProtectionProvider>();

        var encryptedService = new EncryptedCacheService(baseService, dataProtectionProvider, _cachingOptions, encryptionLogger);
        _trackedServices.Add(encryptedService);

        return encryptedService;
    }

    public void Dispose()
    {
        foreach (var service in _trackedServices)
        {
            if (service is IDisposable disposableService)
            {
                disposableService.Dispose();
            }
        }
        _trackedServices.Clear();
    }
}