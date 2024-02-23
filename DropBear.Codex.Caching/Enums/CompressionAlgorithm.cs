namespace DropBear.Codex.Caching.Enums;

/// <summary>
/// Specifies the compression algorithm to use.
/// </summary>
public enum CompressionAlgorithm
{
    /// <summary>
    /// Brotli compression algorithm, efficient for text data.
    /// </summary>
    Brotli, // Corrected spelling
    
    /// <summary>
    /// LZ4 compression algorithm, known for its high-speed and decent compression ratio.
    /// </summary>
    LZ4,
}