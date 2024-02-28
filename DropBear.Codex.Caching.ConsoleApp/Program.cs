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
        var services = new ServiceCollection();
        var configuration = LoadConfiguration();
        ConfigureServices(services, configuration);

        var serviceProvider = services.BuildServiceProvider();
        await TestConfiguredCacheServices(serviceProvider).ConfigureAwait(false);
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
        services.AddCodexCaching(configuration, configure =>
        {
            configure.FasterKvOptions.Enabled = true;
            configure.SqLiteOptions.Enabled = true;
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
            configure.CompressionOptions.Algorithm = CompressionAlgorithm.Lz4;

            // Set the SQLite options
            configure.SqLiteOptions.CacheName = "SQLiteCache";

            // Set the InMemory options
            configure.InMemoryOptions.CacheName = "InMemoryCache";

            // Set the FasterKV options
            configure.FasterKvOptions.CacheName = "FasterKVCache";
        });
    }

    private static async Task TestConfiguredCacheServices(IServiceProvider serviceProvider)
    {
        // Example: Testing multiple cache services based on configuration or logic
        var cacheTypes = new[] { CacheType.InMemory, CacheType.SqLite, CacheType.FasterKv };
        foreach (var cacheType in cacheTypes)
        {
            var cacheService = serviceProvider.GetRequiredService<ICachingServiceFactory>()
                .GetCachingService(cacheType);
            await TestCacheService(cacheService, cacheType.ToString()).ConfigureAwait(false);
        }
    }

    private static async Task TestCacheService(ICacheService cacheService, string serviceName)
    {
        const string Key = "testKey";
        var value = $"testValue for {serviceName}";

        await cacheService.SetAsync(Key, value, TimeSpan.FromMinutes(5)).ConfigureAwait(false);
        var cachedValue = await cacheService.GetAsync<string>(Key).ConfigureAwait(false);

        Console.WriteLine($"{serviceName} Cached Value: {cachedValue}");
    }
}