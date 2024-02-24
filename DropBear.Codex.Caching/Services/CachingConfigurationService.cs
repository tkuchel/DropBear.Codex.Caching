using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Exceptions;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core.Configurations;
using EasyCaching.Serialization.MemoryPack;
using MethodTimer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Caching.Services;

/// <summary>
///     Provides services for configuring EasyCaching based on application-defined options.
/// </summary>
public class CachingConfigurationService : ICachingConfigurationService
{
    private readonly ILogger<CachingConfigurationService> _logger;
    private readonly CachingOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CachingConfigurationService" /> class.
    /// </summary>
    /// <param name="options">The caching configuration options.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public CachingConfigurationService(CachingOptions options, ILogger<CachingConfigurationService>? logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Configures EasyCaching options based on the provided <see cref="CachingOptions" />.
    /// </summary>
    /// <param name="easyCachingOptions">The EasyCaching options to configure.</param>
    [Time]
    public void ConfigureEasyCaching(EasyCachingOptions easyCachingOptions)
    {
        try
        {
            if (_options.InMemoryOptions.Enabled)
                easyCachingOptions.UseInMemory(inMemoryConfig =>
                {
                    // Directly configure InMemoryOptions using _options.InMemoryOptions
                    inMemoryConfig.DBConfig.ExpirationScanFrequency =
                        _options.InMemoryOptions.ExpirationScanFrequency;
                    inMemoryConfig.DBConfig.SizeLimit = _options.InMemoryOptions.SizeLimit;
                }, _options.InMemoryOptions.CacheName);

            if (_options.SQLiteOptions.Enabled)
                easyCachingOptions.UseSQLite(sqliteConfig =>
                {
                    // Directly configure SQLiteOptions using _options.SQLiteOptions
                    sqliteConfig.DBConfig.FilePath = _options.SQLiteOptions.FilePath;
                    sqliteConfig.DBConfig.FileName = _options.SQLiteOptions.FileName;
                }, _options.SQLiteOptions.CacheName);

            if (_options.FasterKVOptions.Enabled)
                easyCachingOptions.UseFasterKv(fasterKVConfig =>
                {
                    // Directly configure FasterKvCachingOptions using _options.FasterKVOptions
                }, _options.FasterKVOptions.CacheName);

            // Implement other caching configurations (SQLite, FasterKV) as needed...

            // Serialization configuration
            ConfigureSerialization(easyCachingOptions, _options.SerializationOptions);

            // Compression configuration
            ConfigureCompression(easyCachingOptions, _options.CompressionOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring caching services.");
            throw new ConfigurationException("Error configuring caching services.", ex);
        }
    }


    [Time]
    private void ConfigureSerialization(EasyCachingOptions options, SerializationOptions serializationOptions)
    {
        try
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
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error configuring serialization.");
            throw new ConfigurationException("Error configuring serialization.", ex);
        }
    }

    [Time]
    private void ConfigureCompression(EasyCachingOptions options, CompressionOptions compressionOptions)
    {
        try
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
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error configuring compression.");
            throw new ConfigurationException("Error configuring compression.", ex);
        }
    }
}