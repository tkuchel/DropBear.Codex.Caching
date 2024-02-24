using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Factories;
using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.Services;
using EasyCaching.Core.Configurations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLogger;
using ZLogger.Providers;

namespace DropBear.Codex.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    
    /// <summary>
    ///     Adds CodexCaching services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configure">Action to configure CachingOptions.</param>
    /// <param name="preloaders">Optional collection of preloaders to be registered.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddCodexCaching(
        this IServiceCollection services,
        Action<CachingOptions> configure,
        IEnumerable<ICachePreloader>? preloaders = null)
    {
        // Conditionally add ZLogger for enhanced logging capabilities.
        ConfigureZLogger(services);
        
        // Register and validate CachingOptions.
        services.AddOptions<CachingOptions>()
            .Configure(configure)
            .ValidateDataAnnotations();

        // Post-configuration for Data Protection based on options.
        services.AddSingleton<IValidateOptions<CachingOptions>, ValidateCachingOptions>();
        services.PostConfigure<CachingOptions>(options =>
        {
            if (options.UseEncryption) ValidateAndSetupDataProtection(services, options);
        });

        // Configure EasyCaching with options including serialization and compression.
        // Defer the actual configuration to a later stage to avoid direct ServiceProvider creation.
        services.AddEasyCaching(options =>
        {
            /* Configuration moved to PostConfigureAll */
        });

        // Register the caching service factory for DI.
        services.AddSingleton<ICachingServiceFactory, CachingServiceFactory>();

        // Register preloaders if provided.
        if (preloaders != null)
            foreach (var preloader in preloaders)
                services.AddSingleton(typeof(ICachePreloader), preloader);

        // Register a hosted service to trigger preloaders after the application starts.
        services.AddHostedService<PreloadingHostedService>();

        // Register the CachingConfigurationService
        services.AddSingleton<ICachingConfigurationService, CachingConfigurationService>();

        // Register a post-configuration action for EasyCachingOptions
        // that depends on ICachingConfigurationService
        services.AddTransient<IConfigureOptions<EasyCachingOptions>, ConfigureEasyCachingOptions>();

        return services;
    }

    private static void ValidateAndSetupDataProtection(IServiceCollection services, CachingOptions options)
    {
        if (string.IsNullOrEmpty(options.EncryptionApplicationName))
            throw new ArgumentException("EncryptionApplicationName is required when UseEncryption is enabled.");

        var keyPath = options.KeyStoragePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            options.EncryptionApplicationName,
            "DataProtectionKeys");

        services.AddDataProtection()
            .SetApplicationName(options.EncryptionApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath));
    }
    
    /// <summary>
    /// Configures ZLogger logging services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add logging services to.</param>
    private static void ConfigureZLogger(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders()
                .SetMinimumLevel(LogLevel.Debug)
                .AddZLoggerConsole(options =>
                {
                    options.UseMessagePackFormatter(formatter =>
                    {
                        formatter.IncludeProperties = IncludeProperties.ParameterKeyValues;
                    });
                })
                .AddZLoggerRollingFile(options =>
                {
                    options.FilePathSelector = (timestamp, sequenceNumber) =>
                        $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";
                    options.RollingInterval = RollingInterval.Day;
                    options.RollingSizeKB = 1024; // 1MB
                });
        });
    }
}



/// <summary>
///     Validates CachingOptions with custom logic.
/// </summary>
internal class ValidateCachingOptions : IValidateOptions<CachingOptions>
{
    public ValidateOptionsResult Validate(string name, CachingOptions options)
    {
        var failures = new List<string>();

        if (options.UseEncryption && string.IsNullOrWhiteSpace(options.EncryptionApplicationName))
            failures.Add($"{nameof(options.EncryptionApplicationName)} is required when encryption is enabled.");

        // Additional validation logic as necessary...

        return failures.Any() ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}