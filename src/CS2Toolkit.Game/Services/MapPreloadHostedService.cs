using CS2Toolkit.Game.Maps;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Game.Services;

internal sealed class MapPreloadHostedService : BackgroundService
{
    private readonly MapDataService _mapDataService;
    private readonly ILogger<MapPreloadHostedService> _logger;

    public MapPreloadHostedService(MapDataService mapDataService, ILogger<MapPreloadHostedService> logger)
    {
        _mapDataService = mapDataService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting map collision mesh preload");
        await _mapDataService.ParseAllMapsAsync(
            message => _logger.LogInformation("{Message}", message),
            stoppingToken);
    }
}
