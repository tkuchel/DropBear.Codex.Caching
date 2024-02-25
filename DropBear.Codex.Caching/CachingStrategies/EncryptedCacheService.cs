using System.Text.Json;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Interfaces;
using MethodTimer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Caching.CachingStrategies;

/// <summary>
///     Provides an encrypted caching service that wraps around a base cache service,
///     adding encryption and decryption to cache operations using the ASP.NET Core Data Protection API.
/// </summary>
public class EncryptedCacheService : ICacheService
{
    private readonly ICacheService _baseCacheService;
    private readonly IDataProtector _dataProtector;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        // Add any necessary JsonSerializerOptions here
    };

    private readonly ILogger<EncryptedCacheService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EncryptedCacheService" /> class.
    /// </summary>
    /// <param name="baseCacheService">The underlying cache service to be wrapped with encryption.</param>
    /// <param name="dataProtectionProvider">The data protection provider used for creating a data protector.</param>
    /// <param name="options">Options for configuring the encryption key and application name.</param>
    /// <param name="logger">An instance of ILogger used for logging within the EncryptedCacheService class.</param>
    public EncryptedCacheService(
        ICacheService baseCacheService,
        IDataProtectionProvider dataProtectionProvider,
        CachingOptions options,
        ILogger<EncryptedCacheService>? logger)
    {
        if (options == null || string.IsNullOrEmpty(options.EncryptionOptions.EncryptionApplicationName))
            throw new ArgumentException("Encryption options must specify an ApplicationName.");

        _baseCacheService = baseCacheService ?? throw new ArgumentNullException(nameof(baseCacheService));
        _dataProtector = dataProtectionProvider.CreateProtector(options.EncryptionOptions.EncryptionApplicationName);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("EncryptedCacheService initialized.");
    }

    /// <summary>
    ///     Asynchronously adds an item to the cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">The type of the item to be cached.</typeparam>
    /// <param name="key">The key for the cached item. This key is used to retrieve the item later.</param>
    /// <param name="value">The item to cache. The item will be serialized and encrypted before storing.</param>
    /// <param name="expiry">
    ///     Optional. The amount of time for which the item should be cached. If not specified, a default
    ///     expiration will be applied.
    /// </param>
    /// <remarks>
    ///     This method serializes and encrypts the provided value before caching it. In case of any error during serialization
    ///     or encryption, the error is logged, and the operation is skipped to allow for graceful degradation. This ensures
    ///     that transient issues do not cause the application to fail but instead degrade gracefully by potentially bypassing
    ///     the cache.
    /// </remarks>
    /// <exception cref="Exception">Throws an exception if an error occurs during the encryption or serialization process.</exception>
    [Time]
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            var encryptedData = _dataProtector.Protect(jsonData);
            await _baseCacheService.SetAsync(key, encryptedData, expiry);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex,
                $"Error encrypting or serializing data for key '{key}'. Operation skipped for graceful degradation.");
            // Consider whether to throw an exception or continue for graceful degradation.
        }
    }

    /// <summary>
    ///     Removes a cached item by its key.
    /// </summary>
    /// <param name="key">The cache key of the item to remove.</param>
    [Time]
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _baseCacheService.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex,
                $"Error removing cache item for key '{key}'. Operation skipped for graceful degradation.");
            // Since this is a removal operation, simply log and degrade gracefully.
        }
    }

    /// <summary>
    ///     Clears all items from the cache.
    /// </summary>
    [Time]
    public async Task FlushAsync()
    {
        try
        {
            await _baseCacheService.FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error flushing cache. Operation skipped for graceful degradation.");
            // Flushing the cache is a broad operation; log the error and degrade gracefully.
        }
    }

    /// <summary>
    ///     Retrieves an item from the cache. If the item is not found and a fallback function is provided,
    ///     the fallback function is invoked to retrieve the item. If the item is still not found or an error occurs,
    ///     the method returns the default value for the type.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve from the cache.</param>
    /// <param name="fallbackFunction">
    ///     An optional function that is invoked to retrieve the item if it is not found in the
    ///     cache. If provided, the result of this function will be returned in case of a cache miss.
    /// </param>
    /// <returns>
    ///     The cached item if found, the result of the fallback function if provided and the item is not found, or the
    ///     default value for the type if the item is not found and no fallback function is provided or if an error occurs.
    /// </returns>
    [Time]
    public async Task<T?> GetAsync<T>(string key, Func<Task<T?>>? fallbackFunction = null)
    {
        try
        {
            var encryptedData = await _baseCacheService.GetAsync<string>(key);
            if (string.IsNullOrEmpty(encryptedData))
            {
                _logger.ZLogInformation($"Cache miss for key {key}.");
                if (fallbackFunction == null) return default;
                _logger.ZLogInformation($"Attempting to retrieve from fallback function for key {key}.");
                return await fallbackFunction();
            }

            var decryptedData = _dataProtector.Unprotect(encryptedData);
            return JsonSerializer.Deserialize<T>(decryptedData);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"Error occurred during cache retrieval or processing for key: {key}.");
            // Attempt to use the fallback function to retrieve the data if provided
            if (fallbackFunction == null) return default;
            try
            {
                _logger.ZLogInformation(
                    $"Attempting to retrieve from fallback function for key {key} after an error.");
                return await fallbackFunction();
            }
            catch (Exception fallbackEx)
            {
                _logger.ZLogError(fallbackEx, $"Fallback function also failed for key: {key}.");
            }

            // Graceful degradation: return default value if fallback is not provided or fails
            return default;
        }
    }
}