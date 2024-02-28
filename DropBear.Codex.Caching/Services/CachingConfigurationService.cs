using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Exceptions;
using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core.Configurations;
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
    /// <param name="options">The EasyCaching options to configure.</param>
    [Time]
    public void ConfigureEasyCaching(EasyCachingOptions options)
    {
        var serName = _options.SerializationOptions.Format switch
        {
            SerializationFormat.MessagePack => Constants.MessagePackSerializerName,
            SerializationFormat.Json => Constants.JsonSerializerName,
            _ => throw new ArgumentOutOfRangeException($"Unsupported serialization format: {_options.SerializationOptions.Format}.",nameof(_options.SerializationOptions.Format))
        };

        try
        {
            if (_options.InMemoryOptions.Enabled)
                options.UseInMemory(inMemoryConfig =>
                {
                    // Directly configure InMemoryOptions using _options.InMemoryOptions
                    inMemoryConfig.DBConfig.ExpirationScanFrequency =
                        _options.InMemoryOptions.ExpirationScanFrequency;
                    inMemoryConfig.DBConfig.SizeLimit = _options.InMemoryOptions.SizeLimit;
                    inMemoryConfig.SerializerName = serName;
                }, _options.InMemoryOptions.CacheName);

            if (_options.SqLiteOptions.Enabled)
                options.UseSQLite(sqliteConfig =>
                {
                    // Directly configure SQLiteOptions using _options.SQLiteOptions
                    sqliteConfig.DBConfig.FilePath = _options.SqLiteOptions.FilePath;
                    sqliteConfig.DBConfig.FileName = _options.SqLiteOptions.FileName;
                    sqliteConfig.SerializerName = serName;
                }, _options.SqLiteOptions.CacheName);

            if (_options.FasterKvOptions.Enabled)
                options.UseFasterKv(fasterKvConfig => { fasterKvConfig.SerializerName = serName; },
                    _options.FasterKvOptions.CacheName);

            // Implement other caching configurations (SQLite, FasterKV) as needed...

            // Serialization configuration
            ConfigureSerialization(options, _options.SerializationOptions);

            // Compression configuration
            ConfigureCompression(options, _options.CompressionOptions);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error configuring caching services.");
            throw new ConfigurationException("Error configuring caching services.", ex);
        }
    }


    [Time]
    private void ConfigureSerialization(EasyCachingOptions options, SerializationOptions serializationOptions)
    {
        try
        {
            if (!serializationOptions.Enabled)
            {
                // Serialization is now a requirement, so throw an error if it's disabled or fallback to a safe, versatile serializer.
                // This defaults to JSON for testing purposes.
                options.WithJson(Constants.JsonSerializerName);
                _logger.ZLogWarning($"Serialization is disabled. Defaulting to JSON serialization.");
                return;
            }

            switch (serializationOptions.Format)
            {
                case SerializationFormat.MessagePack:
                    options.WithMessagePack(Constants
                        .MessagePackSerializerName); // Apply MessagePack serialization globally
                    break;
                case SerializationFormat.Json:
                    options.WithJson(Constants.JsonSerializerName); // Apply JSON serialization globally
                    break;
                // Removed the case for SerializationFormat.None
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported serialization format: {serializationOptions.Format}.",nameof(serializationOptions.Format));
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
                    options.WithCompressor(Constants.BrotliCompressionName); // Globally apply Brotli compression
                    break;
                case CompressionAlgorithm.Lz4:
                    options.WithCompressor(Constants.Lz4CompressionName); // Globally apply LZ4 compression
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported compression algorithm: {compressionOptions.Algorithm}.",nameof(compressionOptions.Algorithm));
            }
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error configuring compression.");
            throw new ConfigurationException("Error configuring compression.", ex);
        }
    }
}
