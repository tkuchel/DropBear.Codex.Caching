namespace DropBear.Codex.Caching.Enums;

/// <summary>
/// Defines the type of cache used.
/// </summary>
public enum CacheType
{
    /// <summary>
    /// A cache stored in memory.
    /// </summary>
    InMemory,
    
    /// <summary>
    /// A cache using FasterKV, optimized for high-performance scenarios.
    /// </summary>
    FasterKV,
    
    /// <summary>
    /// A cache stored using SQLite, suitable for persistent storage.
    /// </summary>
    SQLite
}