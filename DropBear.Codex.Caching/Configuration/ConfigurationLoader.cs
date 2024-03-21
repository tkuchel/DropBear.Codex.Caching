using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DropBear.Codex.Caching.Configuration;

public class ConfigurationLoader(IAppLogger<ConfigurationLoader> logger)
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
                logger.LogInformation(ZString.Format("{0} section loaded from configuration.", sectionName));
            }
            else
            {
                logger.LogWarning(ZString.Format("{0} section not found in configuration.", sectionName));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading CachingOptions from configuration.");
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
            logger.LogInformation("CachingOptions configured programmatically.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error configuring CachingOptions programmatically.");
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
