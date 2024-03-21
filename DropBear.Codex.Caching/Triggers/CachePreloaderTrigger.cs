using Cysharp.Text;
using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.Caching.Triggers;

public static class CachePreloaderTrigger
{
    public static async Task TriggerPreloading(IServiceProvider serviceProvider)
    {
        // Create a scope for resolving cache providers
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger>();

        // Check if preloading has already occured
        if (PreloadingState.PreloadingExecuted)
        {
            logger?.LogInformation("Cache preloading has already been executed, skipping.");
            return; // Preloading has already been executed, skip.
        }

        // Resolve all cache preloaders and logger
        var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();


        // Preloader registration check
        var cachePreloaders = preloaders as ICachePreloader[] ?? preloaders.ToArray();
        if (cachePreloaders.Length is 0)
        {
            logger?.LogInformation("No cache preloaders found.");
            return;
        }

        // Process each preloader
        foreach (var preloader in cachePreloaders)
            try
            {
                await preloader.PreloadAsync().ConfigureAwait(false);
                PreloadingState.PreloadingExecuted = true;
                logger?.LogInformation(ZString.Format("Cache preloading executed for {0}.", preloader.GetType().Name));
            }
            catch (Exception ex)
            {
                PreloadingState.PreloadingExecuted = false;
                logger?.LogError(ex, ZString.Format("Cache preloading failed for {0}.", preloader.GetType().Name));
            }
    }
}
