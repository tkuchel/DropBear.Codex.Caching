using DropBear.Codex.Caching.Enums;
using DropBear.Codex.Caching.Extensions;
using DropBear.Codex.Caching.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Caching.ConsoleApp;

internal class Program
{
    private static async Task Main()
    {
        var services = new ServiceCollection();
        var configuration = LoadConfiguration();
        ConfigureServices(services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        await TestConfiguredCacheServices(serviceProvider);
    }

    private static IConfiguration LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ConfigureCachingOptions(services, configuration);
        //services.AddLogging(builder => builder.AddConsole()); // Example of adding logging
    }

    private static void ConfigureCachingOptions(IServiceCollection services, IConfiguration configuration)
    {
        var preloaders = new List<ICachePreloader> { new ExamplePreloader() };
        services.AddCodexCaching(configuration, configure =>
        {
            configure.FasterKVOptions.Enabled = true;
            configure.SQLiteOptions.Enabled = true;
            configure.InMemoryOptions.Enabled = true;

            // Set the default cache duration to 5 minutes
            configure.DefaultCacheDurationMinutes = TimeSpan.FromMinutes(5);

            // Set the encryption options
            configure.EncryptionOptions.Enabled = true;
            configure.EncryptionOptions.EncryptionApplicationName = "CodexCaching";
            configure.EncryptionOptions.KeyStoragePath = @"C:\Temp\Keys";

            // Set the serialization options
            configure.SerializationOptions.Enabled = true;
            configure.SerializationOptions.Format = SerializationFormat.MessagePack;

            // Set the compression options
            configure.CompressionOptions.Enabled = true;
            configure.CompressionOptions.Algorithm = CompressionAlgorithm.LZ4;

            // Set the SQLite options
            configure.SQLiteOptions.CacheName = "SQLiteCache";

            // Set the InMemory options
            configure.InMemoryOptions.CacheName = "InMemoryCache";

            // Set the FasterKV options
            configure.FasterKVOptions.CacheName = "FasterKVCache";
        },preloaders);
    }

    private static async Task TestConfiguredCacheServices(IServiceProvider serviceProvider)
    {
        // Example: Testing multiple cache services based on configuration or logic
        var cacheTypes = new[] { CacheType.InMemory, CacheType.SQLite, CacheType.FasterKV };
        foreach (var cacheType in cacheTypes)
        {
            var cacheService = serviceProvider.GetRequiredService<ICachingServiceFactory>()
                .GetCachingService(cacheType);
            await TestCacheService(cacheService, cacheType.ToString());
        }
    }

    private static async Task TestCacheService(ICacheService cacheService, string serviceName)
    {
        var key = "testKey";
        var value = $"testValue for {serviceName}";

        await cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var cachedValue = await cacheService.GetAsync<string>(key);

        Console.WriteLine($"{serviceName} Cached Value: {cachedValue}");
    }
}