using System.Text.Json;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace DropBear.Codex.Caching.Services;

/// <summary>
///     Provides an encrypted caching service that wraps around a base cache service,
///     adding encryption and decryption to cache operations using the ASP.NET Core Data Protection API.
/// </summary>
public class EncryptedCacheService : ICacheService
{
    private readonly ICacheService _baseCacheService;
    private readonly IDataProtector _dataProtector;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EncryptedCacheService" /> class.
    /// </summary>
    /// <param name="baseCacheService">The underlying cache service to be wrapped with encryption.</param>
    /// <param name="dataProtectionProvider">The data protection provider used for creating a data protector.</param>
    /// <param name="options">Options for configuring the encryption key and application name.</param>
    public EncryptedCacheService(ICacheService baseCacheService, IDataProtectionProvider dataProtectionProvider,
        IOptions<CachingOptions> options)
    {
        if (options == null || string.IsNullOrEmpty(options.Value.EncryptionApplicationName))
            throw new ArgumentException("Encryption options must specify an ApplicationName.");

        _baseCacheService = baseCacheService ?? throw new ArgumentNullException(nameof(baseCacheService));
        _dataProtector = dataProtectionProvider.CreateProtector(options.Value.EncryptionApplicationName);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var encryptedData = await _baseCacheService.GetAsync<string>(key);
        if (string.IsNullOrEmpty(encryptedData)) return default;

        var decryptedData = _dataProtector.Unprotect(encryptedData);
        return JsonSerializer.Deserialize<T>(decryptedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var jsonData = JsonSerializer.Serialize(value);
        var encryptedData = _dataProtector.Protect(jsonData);
        await _baseCacheService.SetAsync(key, encryptedData, expiry);
    }

    /// <summary>
    ///     Removes a cached item by its key.
    /// </summary>
    /// <param name="key">The cache key of the item to remove.</param>
    public async Task RemoveAsync(string key)
    {
        await _baseCacheService.RemoveAsync(key);
    }

    /// <summary>
    ///     Clears all items from the cache.
    /// </summary>
    public async Task FlushAsync()
    {
        await _baseCacheService.FlushAsync();
    }
}