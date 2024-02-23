using EasyCaching.Core.Configurations;

namespace DropBear.Codex.Caching.Interfaces;

public interface ICachingConfigurationService
{
    void ConfigureEasyCaching(EasyCachingOptions options);
}