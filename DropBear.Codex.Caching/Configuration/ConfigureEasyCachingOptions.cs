using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core.Configurations;
using Microsoft.Extensions.Options;

namespace DropBear.Codex.Caching.Configuration;

public class ConfigureEasyCachingOptions : IConfigureOptions<EasyCachingOptions>
{
    private readonly ICachingConfigurationService _cachingConfigurationService;

    public ConfigureEasyCachingOptions(ICachingConfigurationService cachingConfigurationService)
    {
        _cachingConfigurationService = cachingConfigurationService;
    }

    public void Configure(EasyCachingOptions options)
    {
        _cachingConfigurationService.ConfigureEasyCaching(options);
    }
}