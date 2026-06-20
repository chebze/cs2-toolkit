using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

internal sealed class RadarStateUpdater : BackgroundService
{
    private readonly IReadOnlyGameState _gameState;
    private readonly RadarState _radarState;
    private readonly ToolkitHostSettings _options;
    private readonly ILogger<RadarStateUpdater> _logger;

    public RadarStateUpdater(
        IReadOnlyGameState gameState,
        RadarState radarState,
        IOptions<ToolkitHostSettings> options,
        ILogger<RadarStateUpdater> logger)
    {
        _gameState = gameState;
        _radarState = radarState;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMs = Math.Max(1, _options.MemoryReadIntervalMs);
        _logger.LogInformation("Radar state updater started — interval {Interval}ms", intervalMs);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var snapshot = _gameState.Latest ?? GameSnapshot.Detached;
                _radarState.Update(snapshot.Radar);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Radar state update failed");
            }
        }
    }
}
