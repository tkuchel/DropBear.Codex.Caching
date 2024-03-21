using DropBear.Codex.AppLogger.Extensions;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.Caching.Configuration;
using DropBear.Codex.Caching.Factories;
using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.Services;
using EasyCaching.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Caching.Extensions;

// ReSharper disable once UnusedType.Global
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds CodexCaching services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration"></param>
    /// <param name="configure">Action to configure CachingOptions.</param>
    /// <param name="preloaders">Optional collection of preloaders to be registered.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    // ReSharper disable once UnusedMember.Global
    public static IServiceCollection AddCodexCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<CachingOptions>? configure = null,
        IEnumerable<ICachePreloader>? preloaders = null)
    {
        // Configure AppLogger for enhanced logging capabilities
        services.AddAppLogger();

        var logger = services.BuildServiceProvider().GetService<IAppLogger<ConfigurationLoader>>();
        if (logger is not null)
        {
            var configurationLoader = new ConfigurationLoader(logger);

            // Load or configure CachingOptions
            var cachingOptions = configure is null
                ? configurationLoader.LoadCachingOptions(configuration)
                : configurationLoader.ConfigureCachingOptions(configure);

            // Validate and setup data protection if encryption is enabled
            if (cachingOptions.EncryptionOptions.Enabled) ValidateAndSetupDataProtection(services, cachingOptions);

            // Configure EasyCaching based on CachingOptions
            services.AddEasyCaching(options =>
            {
                var loggerService = services.BuildServiceProvider()
                    .GetService<IAppLogger<CachingConfigurationService>>();
                var cachingConfigurationService = new CachingConfigurationService(cachingOptions, loggerService);
                cachingConfigurationService.ConfigureEasyCaching(options);
            });

            // Register IEasyCachingProviderFactory
            services.AddSingleton<IEasyCachingProviderFactory, DefaultEasyCachingProviderFactory>();

            // Register Compressors
            services.AddLZ4Compressor(Constants.Lz4CompressionName); // Add LZ4 compressor
            services.AddBrotliCompressor(Constants.BrotliCompressionName); // Add Brotli compressor
            // Register MemoryPack


            // Assuming cachingOptions is already instantiated with your configuration loader
            services.AddSingleton<ICachingServiceFactory>(provider =>
                new CachingServiceFactory(
                    provider.GetRequiredService<IEasyCachingProviderFactory>(),
                    cachingOptions, // Direct instance of CachingOptions
                    provider));
        }

        // Register preloaders if provided
        if (preloaders is not null)
            foreach (var preloader in preloaders)
                services.AddSingleton(typeof(ICachePreloader), preloader);

        // Register a hosted service to trigger preloaders after the application starts
        services.AddHostedService<PreloadingHostedService>();

        return services;
    }


    private static void ValidateAndSetupDataProtection(IServiceCollection services, CachingOptions options)
    {
        if (string.IsNullOrEmpty(options.EncryptionOptions.EncryptionApplicationName))
            throw new ArgumentException("EncryptionApplicationName is required when UseEncryption is enabled.",
                nameof(options));

        var keyPath = options.EncryptionOptions.KeyStoragePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            options.EncryptionOptions.EncryptionApplicationName,
            "DataProtectionKeys");

        services.AddDataProtection()
            .SetApplicationName(options.EncryptionOptions.EncryptionApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath));
    }
}
