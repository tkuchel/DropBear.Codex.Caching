# DropBear Codex Caching Library

The DropBear Codex Caching Library is a comprehensive caching solution designed for .NET applications. It supports various caching strategies, including InMemory, SQLite, and FasterKV caches, with advanced features like cache preloading, fallback mechanisms, tag-based invalidation, automatic cache refresh, encrypted cache store, and secure cache access.

## Features

- **Multiple Caching Strategies**: Choose from InMemory, SQLite, and FasterKV caching according to your application needs.
- **Cache Preloading**: Preload frequently accessed data into the cache at application startup to improve performance.
- **Cache Fallback Mechanism**: Automatically fallback to secondary caches or the primary data store on cache misses or failures.
- **Tag-based Invalidation**: Invalidate related cache entries together using tags for efficient cache management.
- **Automatic Cache Refresh**: Refresh cache entries just before they expire to ensure data freshness with minimal latency.
- **Encrypted Cache Store**: Encrypt cache entries to protect sensitive data.
- **Secure Cache Access**: Implement access controls to secure cache operations.

## Getting Started

### Prerequisites

- .NET Core 3.1 or .NET 5/6+
- Microsoft.Extensions.Hosting for integrating with .NET generic host

### Installation

To use the DropBear Codex Caching Library in your project, add the library as a dependency via NuGet or reference the project directly.

```bash
dotnet add package DropBear.Codex.Caching
```

### Configuration

1. **Configure Caching Options**: In your `Startup.cs` or wherever you configure services, add the caching library and configure your preferred caching strategies.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCodexCaching(options =>
    {
        options.InMemoryOptions.Enabled = true;
        options.SQLiteOptions.Enabled = false; // Example configuration
        // Configure other options as needed
    });
}
```

2. **Register and Use Cache Preloaders**: If utilizing cache preloading, define and register preloaders.

```csharp
services.AddCodexCaching(options => { /* Configuration */ }, new List<ICachePreloader> { new MyCachePreloader() });
```

## Usage

Inject `ICacheService` into your services or controllers to interact with the cache.

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
        return await _cacheService.GetAsync<MyData>(key) ?? FetchDataFromStore(key);
    }
}
```

## Contributing

Contributions are welcome! Please read our contributing guidelines and code of conduct before submitting a pull request or opening an issue.

## License

This project is licensed under the GNU Lesser Public General License - see the [LICENSE](https://en.wikipedia.org/wiki/GNU_Lesser_General_Public_License) for details.

## Acknowledgments

- Thanks to the EasyCaching project for providing the core caching infrastructure.
- Contributors and community members who have provided feedback and suggestions.

---

**Note**: As this is a relatively new project, certain features are still in development, this package should be considered a beta version and not used in production environments.