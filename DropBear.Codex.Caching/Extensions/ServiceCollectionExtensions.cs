using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Factories;
using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.Services;
using EasyCaching.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
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
        IConfiguration configuration,
        Action<CachingOptions> configure = null,
        IEnumerable<ICachePreloader>? preloaders = null)
    {
        // Configure ZLogger for enhanced logging capabilities
        ConfigureZLogger(services);

        var logger = services.BuildServiceProvider().GetService<ILogger<ConfigurationLoader>>();
        var configurationLoader = new ConfigurationLoader(logger);

        // Load or configure CachingOptions
        var cachingOptions = configure == null
            ? configurationLoader.LoadCachingOptions(configuration)
            : configurationLoader.ConfigureCachingOptions(configure);

        // Validate and setup data protection if encryption is enabled
        if (cachingOptions.EncryptionOptions.Enabled) ValidateAndSetupDataProtection(services, cachingOptions);

        // Configure EasyCaching based on CachingOptions
        services.AddEasyCaching(options =>
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<CachingConfigurationService>>();
            var cachingConfigurationService = new CachingConfigurationService(cachingOptions, logger);
            cachingConfigurationService.ConfigureEasyCaching(options);
        });

        // Register IEasyCachingProviderFactory
        services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();

        // Assuming cachingOptions is already instantiated with your configuration loader
        services.AddSingleton<ICachingServiceFactory>(provider =>
            new CachingServiceFactory(
                provider.GetRequiredService<IEasyCachingProviderFactory>(),
                cachingOptions, // Direct instance of CachingOptions
                provider));

        // Register preloaders if provided
        if (preloaders != null)
            foreach (var preloader in preloaders)
                services.AddSingleton(typeof(ICachePreloader), preloader);

        // Register a hosted service to trigger preloaders after the application starts
        services.AddHostedService<PreloadingHostedService>();

        return services;
    }


    private static void ValidateAndSetupDataProtection(IServiceCollection services, CachingOptions options)
    {
        if (string.IsNullOrEmpty(options.EncryptionOptions.EncryptionApplicationName))
            throw new ArgumentException("EncryptionApplicationName is required when UseEncryption is enabled.");

        var keyPath = options.EncryptionOptions.KeyStoragePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            options.EncryptionOptions.EncryptionApplicationName,
            "DataProtectionKeys");

        services.AddDataProtection()
            .SetApplicationName(options.EncryptionOptions.EncryptionApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath));
    }

    /// <summary>
    ///     Configures ZLogger logging services.
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
    public ValidateOptionsResult Validate(string? name, CachingOptions options)
    {
        var failures = new List<string>();

        if (options.EncryptionOptions.Enabled &&
            string.IsNullOrWhiteSpace(options.EncryptionOptions.EncryptionApplicationName))
            failures.Add(
                $"{nameof(options.EncryptionOptions.EncryptionApplicationName)} is required when encryption is enabled.");

        // Additional validation logic as necessary...

        return failures.Any() ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}