using DropBear.Codex.Caching.CachingStrategies;
using DropBear.Codex.Caching.Enums;

namespace DropBear.Codex.Caching.Extensions;

public static class CacheTypeExtensions
{
    /// <summary>
    ///     Gets the caching service implementation type associated with a CacheType.
    /// </summary>
    /// <param name="cacheType">The cache type.</param>
    /// <returns>The type of the caching service implementation associated with the specified CacheType.</returns>
    public static Type GetCacheServiceImplementationType(this CacheType cacheType)
    {
        return cacheType switch
        {
            CacheType.InMemory => typeof(InMemoryCachingService),
            CacheType.FasterKV => typeof(FasterKVCachingService),
            CacheType.SQLite => typeof(SQLiteCachingService),
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), $"Unsupported cache type: {cacheType}.")
        };
    }
}