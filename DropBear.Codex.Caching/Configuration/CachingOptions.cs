namespace DropBear.Codex.Caching.Configuration;

public class CachingOptions
{
    public InMemoryOptions InMemory { get; set; } = new();
    public SQLiteOptions SQLite { get; set; } = new();

    public FasterKVOptions FasterKV { get; set; } = new();
    // Add additional caching providers as needed

    public SerializationOptions Serialization { get; set; } = new();
    public CompressionOptions Compression { get; set; } = new();
}