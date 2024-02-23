using DropBear.Codex.Caching.Enums;

namespace DropBear.Codex.Caching.Configuration;

/// <summary>
/// Configuration options for compression.
/// </summary>
public class CompressionOptions
{
    public bool Enabled { get; set; } = false;
    public CompressionAlgorithm Algorithm { get; set; } = CompressionAlgorithm.Brotli;
}