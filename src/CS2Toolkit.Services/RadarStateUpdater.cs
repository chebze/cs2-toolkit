using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Runtime.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

internal sealed class RadarStateUpdater : BackgroundService
{
    private readonly IReadOnlyGameState _gameState;
    private readonly RadarState _radarState;
    private readonly ToolkitHostSettings _options;
    private readonly IRuntimeOrchestrator _orchestrator;
    private readonly ILogger<RadarStateUpdater> _logger;

    public RadarStateUpdater(
        IReadOnlyGameState gameState,
        RadarState radarState,
        IOptions<ToolkitHostSettings> options,
        IRuntimeOrchestrator orchestrator,
        ILogger<RadarStateUpdater> logger)
    {
        _gameState = gameState;
        _radarState = radarState;
        _options = options.Value;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _orchestrator.WaitForPhaseAsync(StartupPhase.Attach, stoppingToken);
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
