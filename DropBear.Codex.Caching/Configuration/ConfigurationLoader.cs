using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Caching.Configuration;

public class ConfigurationLoader(ILogger<ConfigurationLoader> logger)
{
    public CachingOptions LoadCachingOptions(IConfiguration configuration, string sectionName = "CachingOptions")
    {
        var cachingOptions = new CachingOptions();
        try
        {
            var configSection = configuration.GetSection(sectionName);
            if (configSection.Exists())
            {
                configSection.Bind(cachingOptions);
                logger.ZLogInformation($"Successfully loaded {sectionName} configuration.");
            }
            else
            {
                logger.ZLogWarning($"{sectionName} section not found in configuration. Using default values.");
            }
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error loading {sectionName} from configuration.");
        }

        // Optionally, validate and apply defaults after loading
        ValidateAndApplyDefaults(cachingOptions);

        return cachingOptions;
    }

    public CachingOptions ConfigureCachingOptions(Action<CachingOptions>? configureAction)
    {
        var cachingOptions = new CachingOptions();
        try
        {
            configureAction?.Invoke(cachingOptions);
            logger.ZLogInformation($"CachingOptions configured programmatically.");
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error configuring CachingOptions programmatically.");
        }

        // Optionally, validate and apply defaults after configuring
        ValidateAndApplyDefaults(cachingOptions);

        return cachingOptions;
    }

    // ReSharper disable once UnusedParameter.Local
    private static void ValidateAndApplyDefaults(CachingOptions options)
    {
        // Implement validation logic here
        // Apply default values if necessary
        
    }
}