using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core.Configurations;
using EasyCaching.Serialization.MemoryPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DropBear.Codex.Caching.Services;

public class CachingConfigurationService : ICachingConfigurationService
{
    private readonly CachingOptions _options;

    public CachingConfigurationService(IOptions<CachingOptions> options)
    {
        _options = options.Value;
    }

    public void ConfigureEasyCaching(EasyCachingOptions easyCachingOptions)
    {
        // Example: In-memory caching configuration
        if (_options.InMemoryOptions.Enabled)
            easyCachingOptions.UseInMemory(inMemoryConfig =>
            {
                // Directly configure InMemoryOptions using _options.InMemoryOptions
                inMemoryConfig.EnableLogging = _options.InMemoryOptions.EnableLogging;
                inMemoryConfig.DBConfig.ExpirationScanFrequency = _options.InMemoryOptions.ExpirationScanFrequency;
                inMemoryConfig.DBConfig.SizeLimit = _options.InMemoryOptions.SizeLimit;
                inMemoryConfig.DBConfig.EnableReadDeepClone = _options.InMemoryOptions.EnableReadDeepClone;
                inMemoryConfig.DBConfig.EnableWriteDeepClone = _options.InMemoryOptions.EnableWriteDeepClone;
                // Assuming MaxRdSecond, EnableLogging, LockMs, and SleepMs are directly inMemoryConfigurable
                inMemoryConfig.MaxRdSecond = _options.InMemoryOptions.MaxRdSecond;
                inMemoryConfig.EnableLogging = _options.InMemoryOptions.EnableLogging;
                inMemoryConfig.LockMs = _options.InMemoryOptions.LockMs;
                inMemoryConfig.SleepMs = _options.InMemoryOptions.SleepMs;
            }, _options.InMemoryOptions.CacheName);

        if (_options.SQLiteOptions.Enabled)
            easyCachingOptions.UseSQLite(sqliteConfig =>
            {
                // Directly configure SQLiteOptions using _options.SQLiteOptions
                sqliteConfig.DBConfig.FilePath = _options.SQLiteOptions.FilePath;
                sqliteConfig.DBConfig.FileName = _options.SQLiteOptions.FileName;
                sqliteConfig.DBConfig.OpenMode = _options.SQLiteOptions.OpenMode;
                sqliteConfig.DBConfig.CacheMode = _options.SQLiteOptions.CacheMode;
            }, _options.SQLiteOptions.CacheName);

        if (_options.FasterKVOptions.Enabled)
            easyCachingOptions.UseFasterKv(fasterKVConfig =>
            {
                // Directly configure FasterKvCachingOptions using _options.FasterKVOptions
                fasterKVConfig.IndexCount = _options.FasterKVOptions.IndexCount;
                fasterKVConfig.IndexCount = _options.FasterKVOptions.IndexCount;
                fasterKVConfig.MemorySizeBit = _options.FasterKVOptions.MemorySizeBit;
                fasterKVConfig.PageSizeBit = _options.FasterKVOptions.PageSizeBit;
                fasterKVConfig.ReadCacheMemorySizeBit = _options.FasterKVOptions.ReadCacheMemorySizeBit;
                fasterKVConfig.ReadCachePageSizeBit = _options.FasterKVOptions.ReadCachePageSizeBit;
                fasterKVConfig.LogPath = _options.FasterKVOptions.LogPath;
                // Assuming CustomStore is correctly typed and assignable
                fasterKVConfig.CustomStore = _options.FasterKVOptions.CustomStore;
            }, _options.FasterKVOptions.CacheName);

        // Implement other caching configurations (SQLite, FasterKV) similarly...

        // Serialization configuration
        ConfigureSerialization(easyCachingOptions, _options.SerializationOptions);

        // Compression configuration
        ConfigureCompression(easyCachingOptions, _options.CompressionOptions);
    }

    private static void ConfigureSerialization(EasyCachingOptions options, SerializationOptions serializationOptions)
    {
        if (!serializationOptions.Enabled) return;

        switch (serializationOptions.Format)
        {
            case SerializationFormat.MessagePack:
                options.WithMessagePack(); // Apply MessagePack serialization globally
                break;
            case SerializationFormat.MemoryPack:
                options.WithMemoryPack(); // Apply MemoryPack serialization globally
                break;
            case SerializationFormat.Json:
                options.WithJson(); // Apply JSON serialization globally
                break;
            case SerializationFormat.None:
                // No serialization configuration applied
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serializationOptions.Format),
                    $"Unsupported serialization format: {serializationOptions.Format}.");
        }
    }

    private static void ConfigureCompression(EasyCachingOptions options, CompressionOptions compressionOptions)
    {
        if (!compressionOptions.Enabled) return;

        switch (compressionOptions.Algorithm)
        {
            case CompressionAlgorithm.Brotli:
                options.WithCompressor("brotli"); // Globally apply Brotli compression
                break;
            case CompressionAlgorithm.LZ4:
                options.WithCompressor("lz4"); // Globally apply LZ4 compression
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(compressionOptions.Algorithm),
                    $"Unsupported compression algorithm: {compressionOptions.Algorithm}.");
        }
    }
}