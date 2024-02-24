using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Extensions;
using DropBear.Codex.Caching.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Caching.ConsoleApp;

internal class Program
{
    static void Main()
    {
        // Setup DI
        var services = new ServiceCollection();
        ConfigureServices(services);

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Use the caching service factory to get a cache service
        var cacheServiceFactory = serviceProvider.GetService<ICachingServiceFactory>();
        var cacheService = cacheServiceFactory.GetCachingService(CacheType.InMemory); // Choose CacheType as needed
        TestCacheService(cacheService).Wait();
    }

    static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        //services.AddLogging(configure => configure.AddConsole())
        //		.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

        // Add Codex Caching
        services.AddCodexCaching(setup =>
        {
            setup.FasterKVOptions.Enabled = false;
            setup.InMemoryOptions.Enabled = true;
            setup.SQLiteOptions.Enabled = false;
		
            setup.SerializationOptions.Enabled = false;
            setup.SerializationOptions.Format = DropBear.Codex.Caching.Enums.SerializationFormat.None;
		
            setup.CompressionOptions.Enabled = false;
            setup.CompressionOptions.Algorithm = CompressionAlgorithm.Brotli;
		
            setup.EncryptionOptions.Enabled = false;
            setup.EncryptionOptions.EncryptionApplicationName = "";
            setup.EncryptionOptions.KeyStoragePath = null;

        });

        // Register any other services your caching setup requires
    }

    static async Task TestCacheService(ICacheService cacheService)
    {
        string key = "testKey";
        string value = "testValue";

        // Set a value in the cache
        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Retrieve the value from the cache
        var cachedValue = await cacheService.GetAsync<string>(key);

        // Output the result
        Console.WriteLine($"Cached Value: {cachedValue}");
    }

}

