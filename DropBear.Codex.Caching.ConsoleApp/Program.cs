using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Extensions;
using DropBear.Codex.Caching.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.Caching.ConsoleApp;

internal class Program
{
    private static async Task Main()
    {
        // Setup DI
        var services = new ServiceCollection();

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        ConfigureServices(services, configuration);

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Use the caching service factory to get a cache service
        var cacheServiceFactory = serviceProvider.GetRequiredService<ICachingServiceFactory>();
        var cacheService = cacheServiceFactory.GetCachingService(CacheType.InMemory);
        await TestCacheService(cacheService);
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCodexCaching(configuration, configure =>
        {
            configure.FasterKVOptions.Enabled = false;
            configure.SQLiteOptions.Enabled = false;
            configure.InMemoryOptions.Enabled = true;
            
            // Set the default cache duration to 5 minutes
            configure.DefaultCacheDurationMinutes = TimeSpan.FromMinutes(5);
            
            // Set the encryption options
            configure.EncryptionOptions.Enabled = false;
            configure.EncryptionOptions.EncryptionApplicationName = "CodexCaching";
            configure.EncryptionOptions.KeyStoragePath = "C:\\Temp\\CodexCaching";
            
            // Set the serialization options
            configure.SerializationOptions.Enabled = false;
            configure.SerializationOptions.Format = SerializationFormat.None;
            
            // Set the compression options
            configure.CompressionOptions.Enabled = false;
            configure.CompressionOptions.Algorithm = CompressionAlgorithm.Brotli;
            
            // Set the SQLite options
            configure.SQLiteOptions.CacheName = "SQLiteCache";
            
            // Set the InMemory options
            configure.InMemoryOptions.CacheName = "InMemoryCache";
            
            // Set the FasterKV options
            configure.FasterKVOptions.CacheName = "FasterKVCache";
            
        }); 
        
        // Configure logging, etc.
        //services.AddLogging(builder => builder.AddConsole());
    }


    private static async Task TestCacheService(ICacheService cacheService)
    {
        var key = "testKey";
        var value = "testValue";

        // Set a value in the cache
        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Retrieve the value from the cache
        var cachedValue = await cacheService.GetAsync<string>(key);

        // Output the result
        Console.WriteLine($"Cached Value: {cachedValue}");
    }
}