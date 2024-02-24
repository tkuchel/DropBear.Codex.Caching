using DropBear.Codex.Caching.Interfaces;
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
        _logger.ZLogInformation($"Cache preloading started.");
        using var scope = _serviceProvider.CreateScope();
        var preloaders = scope.ServiceProvider.GetServices<ICachePreloader>();
        foreach (var preloader in preloaders)
            try
            {
                await preloader.PreloadAsync();
                _logger.ZLogInformation($"Preloading completed for {preloader.GetType().Name}.");
            }
            catch (Exception ex)
            {
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