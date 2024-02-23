using DropBear.Codex.Caching.CachingStrategies;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.Services;
using EasyCaching.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
// Needed for GetRequiredService

namespace DropBear.Codex.Caching.Factories;

/// <summary>
///     Factory for creating caching service instances based on the specified cache type.
///     Optionally supports encryption based on configuration settings.
/// </summary>
public class CachingServiceFactory : ICachingServiceFactory
{
    private readonly IOptions<CachingOptions> _cacheOptions;
    private readonly IEasyCachingProviderFactory _providerFactory;
    private readonly IServiceProvider _serviceProvider; // Used to access IDataProtectionProvider

    /// <summary>
    ///     Initializes a new instance of the <see cref="CachingServiceFactory" /> class.
    /// </summary>
    /// <param name="providerFactory">The EasyCaching provider factory used to create caching providers.</param>
    /// <param name="cachingOptions">The options for configuring caching services.</param>
    /// <param name="serviceProvider">The service provider for resolving additional services like data protection.</param>
    public CachingServiceFactory(IEasyCachingProviderFactory providerFactory, IOptions<CachingOptions> cachingOptions,
        IServiceProvider serviceProvider)
    {
        _providerFactory = providerFactory;
        _cacheOptions = cachingOptions;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Creates and returns an <see cref="ICacheService" /> instance based on the specified <see cref="CacheType" />.
    ///     If encryption is enabled in the caching options, wraps the cache service with encryption functionality.
    /// </summary>
    /// <param name="cacheType">The type of cache to create.</param>
    /// <returns>
    ///     An instance of an object that implements <see cref="ICacheService" />, configured according to the cache type
    ///     and encryption settings.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported <see cref="CacheType" /> is specified.</exception>
    public ICacheService GetCachingService(CacheType cacheType)
    {
        ICacheService baseService = cacheType switch
        {
            CacheType.InMemory => new InMemoryCachingService(_providerFactory, _cacheOptions),
            CacheType.FasterKV => new FasterKVCachingService(_providerFactory, _cacheOptions),
            CacheType.SQLite => new SQLiteCachingService(_providerFactory, _cacheOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), $"Unsupported cache type: {cacheType}.")
        };

        if (!_cacheOptions.Value.UseEncryption) return baseService;
        
        // Retrieve the IDataProtectionProvider from the service provider
        var dataProtectionProvider = _serviceProvider.GetRequiredService<IDataProtectionProvider>();
        
        // Create a data protector using the application name specified in EncryptionApplicationName
        var dataProtector = dataProtectionProvider.CreateProtector(_cacheOptions.Value.EncryptionApplicationName);
        
        // Wrap the base service with encryption functionality, passing the dataProtector
        return new EncryptedCacheService(baseService, dataProtector, _cacheOptions);
    }
}