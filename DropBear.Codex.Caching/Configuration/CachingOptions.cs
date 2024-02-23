namespace DropBear.Codex.Caching.Configuration;

/// <summary>
///     Provides caching configuration options.
/// </summary>
public class CachingOptions
{
    public TimeSpan DefaultCacheDurationMinutes { get; } = TimeSpan.FromMinutes(30);
    public InMemoryOptions InMemoryOptions { get; set; } = new();
    public SQLiteOptions SQLiteOptions { get; set; } = new();
    public FasterKVOptions FasterKVOptions { get; set; } = new();
    public SerializationOptions SerializationOptions { get; set; } = new();
    public CompressionOptions CompressionOptions { get; set; } = new();
}