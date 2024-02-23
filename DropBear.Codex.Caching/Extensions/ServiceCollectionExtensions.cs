using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Factories;
using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.Services;
using EasyCaching.Core.Configurations;
using EasyCaching.Serialization.MemoryPack;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCodexCaching(this IServiceCollection services, Action<CachingOptions> configure,
        IEnumerable<ICachePreloader> preloaders = null)
    {
        var options = new CachingOptions();
        configure(options);

        // Configure EasyCaching with options including serialization and compression
        services.AddEasyCaching(easyCachingOptions =>
        {
            if (options.InMemoryOptions.Enabled)
                AddInMemoryCaching(easyCachingOptions, options);
            if (options.SQLiteOptions.Enabled)
                AddSQLiteCaching(easyCachingOptions, options);
            if (options.FasterKVOptions.Enabled)
                AddFasterKVCaching(easyCachingOptions, options);

            // Apply global configurations for serialization and compression
            ConfigureSerialization(easyCachingOptions, options.SerializationOptions);
            ConfigureCompression(easyCachingOptions, options.CompressionOptions);
        });

        // Register the caching service factory for DI
        services.AddSingleton<ICachingServiceFactory, CachingServiceFactory>();

        // Register preloaders if provided
        if (preloaders != null)
            foreach (var preloader in preloaders)
                // This registers each preloader for DI
                services.AddSingleton(typeof(ICachePreloader), preloader.GetType());

        // Register a hosted service to trigger preloaders after the application starts
        services.AddHostedService<PreloadingHostedService>();

        // Register the caching options for DI
        services.Configure(configure);

        return services;
    }


    private static void AddInMemoryCaching(EasyCachingOptions options, CachingOptions config)
    {
        options.UseInMemory(inMemoryConfig =>
        {
            // Directly configure InMemoryOptions using config.InMemoryOptions
            inMemoryConfig.EnableLogging = config.InMemoryOptions.EnableLogging;
            inMemoryConfig.DBConfig.ExpirationScanFrequency = config.InMemoryOptions.ExpirationScanFrequency;
            inMemoryConfig.DBConfig.SizeLimit = config.InMemoryOptions.SizeLimit;
            inMemoryConfig.DBConfig.EnableReadDeepClone = config.InMemoryOptions.EnableReadDeepClone;
            inMemoryConfig.DBConfig.EnableWriteDeepClone = config.InMemoryOptions.EnableWriteDeepClone;
            // Assuming MaxRdSecond, EnableLogging, LockMs, and SleepMs are directly inMemoryConfigurable
            inMemoryConfig.MaxRdSecond = config.InMemoryOptions.MaxRdSecond;
            inMemoryConfig.EnableLogging = config.InMemoryOptions.EnableLogging;
            inMemoryConfig.LockMs = config.InMemoryOptions.LockMs;
            inMemoryConfig.SleepMs = config.InMemoryOptions.SleepMs;
        }, config.InMemoryOptions.CacheName);
    }


    private static void AddSQLiteCaching(EasyCachingOptions options, CachingOptions config)
    {
        options.UseSQLite(sqliteConfig =>
        {
            // Directly configure SQLiteOptions using config.SQLiteOptions
            sqliteConfig.DBConfig.FilePath = config.SQLiteOptions.FilePath;
            sqliteConfig.DBConfig.FileName = config.SQLiteOptions.FileName;
            sqliteConfig.DBConfig.OpenMode = config.SQLiteOptions.OpenMode;
            sqliteConfig.DBConfig.CacheMode = config.SQLiteOptions.CacheMode;
        }, config.SQLiteOptions.CacheName);
    }


    private static void AddFasterKVCaching(EasyCachingOptions options, CachingOptions config)
    {
        options.UseFasterKv(fasterKVConfig =>
        {
            // Directly configure FasterKvCachingOptions using config.FasterKVOptions
            fasterKVConfig.IndexCount = config.FasterKVOptions.IndexCount;
            fasterKVConfig.IndexCount = config.FasterKVOptions.IndexCount;
            fasterKVConfig.MemorySizeBit = config.FasterKVOptions.MemorySizeBit;
            fasterKVConfig.PageSizeBit = config.FasterKVOptions.PageSizeBit;
            fasterKVConfig.ReadCacheMemorySizeBit = config.FasterKVOptions.ReadCacheMemorySizeBit;
            fasterKVConfig.ReadCachePageSizeBit = config.FasterKVOptions.ReadCachePageSizeBit;
            fasterKVConfig.LogPath = config.FasterKVOptions.LogPath;
            // Assuming CustomStore is correctly typed and assignable
            fasterKVConfig.CustomStore = config.FasterKVOptions.CustomStore;
        }, config.FasterKVOptions.CacheName);
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