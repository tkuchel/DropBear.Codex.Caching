using DropBear.Codex.Caching.Enums;

namespace DropBear.Codex.Caching.Interfaces;

/// <summary>
/// Defines a factory interface for creating caching service instances based on a specified cache type.
/// </summary>
public interface ICachingServiceFactory
{
    /// <summary>
    /// Retrieves a caching service instance configured for a specific cache type.
    /// </summary>
    /// <param name="cacheType">The type of cache for which the service is requested.</param>
    /// <returns>An instance of a class implementing <see cref="ICacheService"/>, configured for the specified cache type.</returns>
    ICacheService GetCachingService(CacheType cacheType);
}

