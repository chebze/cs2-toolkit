using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Runtime.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

internal sealed class FeatureCoordinator : BackgroundService
{
    private readonly IReadOnlyGameState _gameState;
    private readonly IActiveConfiguration _configuration;
    private readonly IReadOnlyList<IFeatureService> _features;
    private readonly IInputSimulator _input;
    private readonly IOverlayComposer _composer;
    private readonly IOverlayFrameSink _frameSink;
    private readonly IOverlayViewport _viewport;
    private readonly ToolkitHostSettings _options;
    private readonly IRuntimeOrchestrator _orchestrator;
    private readonly ILogger<FeatureCoordinator> _logger;

    public FeatureCoordinator(
        IReadOnlyGameState gameState,
        IActiveConfiguration configuration,
        IEnumerable<IFeatureService> features,
        IInputSimulator input,
        IOverlayComposer composer,
        IOverlayFrameSink frameSink,
        IOverlayViewport viewport,
        IOptions<ToolkitHostSettings> options,
        IRuntimeOrchestrator orchestrator,
        ILogger<FeatureCoordinator> logger)
    {
        _gameState = gameState;
        _configuration = configuration;
        _features = features.ToList();
        _input = input;
        _composer = composer;
        _frameSink = frameSink;
        _viewport = viewport;
        _options = options.Value;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _orchestrator.WaitForPhaseAsync(StartupPhase.Attach, stoppingToken);

        var intervalMs = Math.Max(1, _options.MemoryReadIntervalMs);
        _logger.LogInformation("Feature coordinator started — tick interval {Interval}ms", intervalMs);
        _orchestrator.CompletePhase(StartupPhase.Features);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                ProcessTick(_gameState.Latest ?? GameSnapshot.Detached);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Feature coordinator tick failed");
            }
        }
    }

    private void ProcessTick(GameSnapshot snapshot)
    {
        var weaponId = (ushort)(snapshot.LocalPlayer?.ActiveWeaponId.Value ?? 0);
        var settings = _configuration.Current;
        var weaponSettings = _configuration.ResolveWeapon(weaponId);

        var context = new FeatureContext
        {
            Snapshot = snapshot,
            Settings = settings,
            WeaponSettings = weaponSettings,
            Input = _input
        };

        foreach (var feature in _features)
        {
            if (!feature.IsEnabled)
                continue;

            try
            {
                feature.OnSnapshot(context);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Feature {FeatureId} failed during snapshot processing", feature.Id);
            }
        }

        var frame = _composer.Compose(snapshot, _viewport.Width, _viewport.Height);
        _frameSink.Publish(frame);
    }
}
