using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

internal sealed class OverlayPipelineHostedService : BackgroundService
{
    private readonly IReadOnlyGameState _gameState;
    private readonly IOverlayComposer _composer;
    private readonly IOverlayFrameSink _frameSink;
    private readonly IOverlayViewport _viewport;
    private readonly ToolkitHostSettings _options;
    private readonly ILogger<OverlayPipelineHostedService> _logger;

    public OverlayPipelineHostedService(
        IReadOnlyGameState gameState,
        IOverlayComposer composer,
        IOverlayFrameSink frameSink,
        IOverlayViewport viewport,
        IOptions<ToolkitHostSettings> options,
        ILogger<OverlayPipelineHostedService> logger)
    {
        _gameState = gameState;
        _composer = composer;
        _frameSink = frameSink;
        _viewport = viewport;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMs = Math.Max(1, _options.MemoryReadIntervalMs);
        _logger.LogInformation("Overlay pipeline started — compose interval {Interval}ms", intervalMs);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var snapshot = _gameState.Latest ?? GameSnapshot.Detached;
                var frame = _composer.Compose(snapshot, _viewport.Width, _viewport.Height);
                _frameSink.Publish(frame);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Overlay composition failed");
            }
        }
    }
}
