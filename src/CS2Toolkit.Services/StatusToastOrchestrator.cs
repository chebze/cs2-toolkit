using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Game.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Runtime.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

internal sealed class StatusToastOrchestrator : BackgroundService
{
    private readonly IReadOnlyGameState _gameState;
    private readonly IGameAttachment _attachment;
    private readonly IStatusToastPublisher _toasts;
    private readonly IRuntimeOrchestrator _orchestrator;
    private readonly ToolkitHostSettings _options;
    private readonly ILogger<StatusToastOrchestrator> _logger;
    private bool _wasAttached;

    public StatusToastOrchestrator(
        IReadOnlyGameState gameState,
        IGameAttachment attachment,
        IStatusToastPublisher toasts,
        IRuntimeOrchestrator orchestrator,
        IOptions<ToolkitHostSettings> options,
        ILogger<StatusToastOrchestrator> logger)
    {
        _gameState = gameState;
        _attachment = attachment;
        _toasts = toasts;
        _orchestrator = orchestrator;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _orchestrator.WaitForPhaseAsync(StartupPhase.Input, stoppingToken);

        var intervalMs = Math.Max(1, _options.MemoryReadIntervalMs);
        _logger.LogInformation("Status toast orchestrator started — interval {Interval}ms", intervalMs);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                UpdateToasts(_gameState.Latest ?? GameSnapshot.Detached);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Status toast update failed");
            }
        }
    }

    private void UpdateToasts(GameSnapshot snapshot)
    {
        if (!_attachment.IsAttached)
        {
            if (_wasAttached)
                _toasts.Clear();

            _wasAttached = false;
            return;
        }

        if (!_wasAttached)
            _toasts.ClearPersistent();

        _wasAttached = true;

        if (snapshot.IsInMatch)
            _toasts.Clear();
    }
}
