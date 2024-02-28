namespace DropBear.Codex.Caching.Enums;

/// <summary>
/// Indicates the serialization format to use for caching.
/// </summary>
public enum SerializationFormat
{
   
    /// <summary>
    /// JSON serialization format, widely used for its readability and compatibility.
    /// </summary>
    Json,
    
    /// <summary>
    /// MessagePack serialization, optimized for performance and compactness.
    /// </summary>
    MessagePack,
}