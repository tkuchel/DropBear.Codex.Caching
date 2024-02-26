# DropBear Codex Caching Library

The DropBear Codex Caching Library offers a robust and flexible caching solution tailored for .NET applications. It
supports diverse caching strategies, such as InMemory, SQLite, and FasterKV caches. Enhanced with features like cache
preloading, dynamic fallback mechanisms, tag-based invalidation, automatic cache refresh, encrypted cache storage, and
secure cache access, it aims to optimize performance and ensure data security across .NET applications.

## Features

- **Versatile Caching Strategies**: Utilize InMemory, SQLite, and FasterKV caching based on your application's specific
  requirements.
- **Cache Preloading**: Elevate performance by preloading critical data into the cache upon application startup.
- **Dynamic Cache Fallback Mechanism**: Seamlessly fallback to alternative caches or direct data sources in the event of
  cache misses or operational failures.
- **Encrypted Cache Storage**: Secure sensitive information with encrypted cache entries.

## Getting Started

### Prerequisites

- .NET Core 3.1 or .NET 5/6/7+
- Microsoft.Extensions.Hosting integration for leveraging .NET's generic host capabilities.

### Installation

Incorporate the DropBear Codex Caching Library into your project by adding it as a NuGet package dependency or by
directly referencing the project.

```bash
dotnet add package DropBear.Codex.Caching
```

### Configuration

1. **Set Up Caching Options**: Initialize the caching library within your `Startup.cs` or an equivalent configuration
   setup, specifying your caching strategy preferences.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ConfigureCachingOptions(services, Configuration);
}

private static void ConfigureCachingOptions(IServiceCollection services, IConfiguration configuration)
    {
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
        },preloaders: new List<ICachePreloader> { new MyCachePreloader() });
    }
```

2. **Incorporate Cache Preloaders**: For employing cache preloading, define preloaders and add them during
   configuration.

```csharp
services.AddCodexCaching(options => { /* Configuration details */ }, preloaders: new List<ICachePreloader> { new MyCachePreloader() });
```

## Usage

Leverage `ICacheService` within your services or controllers to manage cache interactions efficiently.

```csharp
public class MyService
{
    private readonly ICacheService _cacheService;

    public MyService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<MyData> GetDataAsync(string key)
    {
        return await _cacheService.GetAsync<MyData>(key, () => FetchDataFromStore(key));
    }
}
```

## Contributing

We warmly welcome contributions! To maintain a constructive and inclusive community, we ask that you first review our
contributing guidelines and code of conduct before submitting pull requests or issues.

## License

This project adheres to the GNU Lesser General Public License. For more details, refer to
the [LICENSE](https://en.wikipedia.org/wiki/GNU_Lesser_General_Public_License) page.

## Acknowledgments

- Special thanks to the EasyCaching project for the foundational caching mechanisms.
- Appreciation for our contributors and community members for their insightful feedback and suggestions.

---

**Disclaimer**: As this project is under active development, features might be subject to change. The current version
should be considered beta and not yet suited for production use.
