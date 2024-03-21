using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.Caching.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DropBear.Codex.Caching.Services;

public class PreloadingHostedService(IServiceProvider serviceProvider, IAppLogger<PreloadingHostedService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Cache preloading started.");
        using var scope = serviceProvider.CreateScope();
        var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();
        foreach (var preloader in preloaders)
            try
            {
                await preloader.PreloadAsync().ConfigureAwait(false);
                logger.LogInformation(ZString.Format("Cache preloading for {0} completed.", preloader.GetType().Name));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ZString.Format("Cache preloading for {0} failed.", preloader.GetType().Name));
            }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Cache preloading service stopped.");
        return Task.CompletedTask;
    }
}
