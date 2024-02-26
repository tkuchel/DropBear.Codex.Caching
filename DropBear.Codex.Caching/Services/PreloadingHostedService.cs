using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.State;
using MethodTimer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Caching.Services;

public class PreloadingHostedService : IHostedService
{
    private readonly ILogger<PreloadingHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PreloadingHostedService(IServiceProvider serviceProvider, ILogger<PreloadingHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    [Time]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Log service start
        _logger.ZLogInformation($"Cache preloading started.");
        
        // Check is preloading has already occured by checking the preloadingstate.
        if (PreloadingState.PreloadingExecuted)
        {
            _logger.ZLogInformation($"Cache preloading has already been executed, skipping.");
            return; // Preloading has already been executed, skip.
        }
        
        // Create a scope and resolve all cache preloaders
        using var scope = _serviceProvider.CreateScope();
        var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();
        
        // For each preloader, execute the preload method
        foreach (var preloader in preloaders)
            try
            {
                await preloader.PreloadAsync();
                PreloadingState.PreloadingExecuted = true;
                _logger.ZLogInformation($"Preloading completed for {preloader.GetType().Name}.");
            }
            catch (Exception ex)
            {
                PreloadingState.PreloadingExecuted = false;
                _logger.ZLogError(ex, $"Error preloading cache for {preloader.GetType().Name}.");
            }
    }
    [Time]
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.ZLogInformation($"Cache preloading service stopped.");
        return Task.CompletedTask;
    }
}