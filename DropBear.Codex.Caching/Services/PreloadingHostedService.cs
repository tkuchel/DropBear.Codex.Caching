using DropBear.Codex.Caching.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DropBear.Codex.Caching.Services;

public class PreloadingHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public PreloadingHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();

        foreach (var preloader in preloaders)
        {
            await preloader.PreloadAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}