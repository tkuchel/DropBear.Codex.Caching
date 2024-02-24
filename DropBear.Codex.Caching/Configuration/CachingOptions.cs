namespace DropBear.Codex.Caching.Configuration;

/// <summary>
///     Provides caching configuration options, including settings for optional encryption.
/// </summary>
public class CachingOptions
{
    /// <summary>
    ///     Gets the default cache duration in minutes.
    /// </summary>
    public TimeSpan DefaultCacheDurationMinutes { get; } = TimeSpan.FromMinutes(30);

    /// <summary>
    ///     Gets or sets the configuration options for in-memory caching.
    /// </summary>
    public InMemoryOptions InMemoryOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the configuration options for SQLite caching.
    /// </summary>
    public SQLiteOptions SQLiteOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the configuration options for FasterKV caching.
    /// </summary>
    public FasterKVOptions FasterKVOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the options for serialization in caching.
    /// </summary>
    public SerializationOptions SerializationOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the options for compression in caching.
    /// </summary>
    public CompressionOptions CompressionOptions { get; set; } = new();
    
    /// <summary>
    ///     Gets or sets the options for encryption in caching.
    /// </summary>
    public EncryptionOptions EncryptionOptions { get; set; } = new();
}