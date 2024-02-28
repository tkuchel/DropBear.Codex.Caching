using DropBear.Codex.Caching.Interfaces;
using EasyCaching.Core.Configurations;
using Microsoft.Extensions.Options;

namespace DropBear.Codex.Caching.Configuration;

public class ConfigureEasyCachingOptions(ICachingConfigurationService cachingConfigurationService)
    : IConfigureOptions<EasyCachingOptions>
{
    public void Configure(EasyCachingOptions options)
    {
        cachingConfigurationService.ConfigureEasyCaching(options);
    }
}