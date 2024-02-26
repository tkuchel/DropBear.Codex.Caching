using DropBear.Codex.Caching.Interfaces;

namespace DropBear.Codex.Caching.ConsoleApp;

public class ExamplePreloader : ICachePreloader
{
    public async Task PreloadAsync()
    {
        // Preload some data into the cache
        await Task.CompletedTask;
    }
}