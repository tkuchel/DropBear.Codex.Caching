namespace DropBear.Codex.Caching.Enums;

/// <summary>
/// Indicates the serialization format to use for caching.
/// </summary>
public enum SerializationFormat
{
    /// <summary>
    /// No serialization, used for raw byte caching.
    /// </summary>
    None,
    
    /// <summary>
    /// JSON serialization format, widely used for its readability and compatibility.
    /// </summary>
    Json,
    
    /// <summary>
    /// MessagePack serialization, optimized for performance and compactness.
    /// </summary>
    MessagePack,
    
    /// <summary>
    /// MemoryPack serialization, designed for high-speed serialization of .NET objects.
    /// </summary>
    MemoryPack
}