namespace DropBear.Codex.Caching.Interfaces;

/// <summary>
/// Provides methods for cache management.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves an item from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <returns>The cached item, or null if not found.</returns>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// Adds an item to the cache with an optional expiry.
    /// </summary>
    /// <typeparam name="T">The type of the item to add.</typeparam>
    /// <param name="key">The key under which to store the item.</param>
    /// <param name="value">The item to cache.</param>
    /// <param name="expiry">Optional. The time after which the cache entry should expire.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    
    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Clears all items from the cache.
    /// </summary>
    Task FlushAsync();
}