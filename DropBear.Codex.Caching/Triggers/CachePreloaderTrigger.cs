using DropBear.Codex.Caching.Interfaces;
using DropBear.Codex.Caching.State;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Caching.Triggers;

public static class CachePreloaderTrigger
{
    public static async Task TriggerPreloading(IServiceProvider serviceProvider)
    {
        // Create a scope for resolving cache providers
        var scope = serviceProvider.CreateAsyncScope();
        // Create a scope for resolving cache providers
        await using (scope.ConfigureAwait(false))
        {
            // Check if preloading has already occured
            if (PreloadingState.PreloadingExecuted) return; // Preloading has already been executed, skip.

            // Resolve all cache preloaders and logger
            var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();


            // Preloader registration check
            var cachePreloaders = preloaders as ICachePreloader[] ?? preloaders.ToArray();
            if (cachePreloaders.Length is 0) return;

            // Process each preloader
            foreach (var preloader in cachePreloaders)
                try
                {
                    await preloader.PreloadAsync().ConfigureAwait(false);
                    PreloadingState.PreloadingExecuted = true;
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    PreloadingState.PreloadingExecuted = false;
                }
        }
    }
}
