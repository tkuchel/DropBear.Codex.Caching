using DropBear.Codex.Caching.Interfaces;
using MethodTimer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Caching.Services;

public class PreloadingHostedService(IServiceProvider serviceProvider, ILogger<PreloadingHostedService> logger)
    : IHostedService
{
    [Time]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.ZLogInformation($"Cache preloading started.");
        using var scope = serviceProvider.CreateScope();
        var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();
        foreach (var preloader in preloaders)
            try
            {
                await preloader.PreloadAsync().ConfigureAwait(false);
                logger.ZLogInformation($"Preloading completed for {preloader.GetType().Name}.");
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Error preloading cache for {preloader.GetType().Name}.");
            }
    }
    [Time]
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.ZLogInformation($"Cache preloading service stopped.");
        return Task.CompletedTask;
    }
}