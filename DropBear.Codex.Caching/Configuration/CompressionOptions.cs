namespace DropBear.Codex.Caching.Configuration;

public class CompressionOptions
{
    public bool Enabled { get; set; } = false; // Whether to enable compression

    public string Algorithm { get; set; } = "Gzip"; // Default compression algorithm: "Gzip", "Brotli", etc.
    // Possible extension point for custom compression settings
}