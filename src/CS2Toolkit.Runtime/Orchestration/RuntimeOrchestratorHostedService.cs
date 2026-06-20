using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Game.Abstractions;
using CS2Toolkit.Game.Maps;
using CS2Toolkit.Runtime.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Runtime.Orchestration;

internal sealed class RuntimeOrchestratorHostedService : BackgroundService
{
    private readonly IRuntimeOrchestrator _orchestrator;
    private readonly IOffsetProvider _offsetProvider;
    private readonly IOverlayRenderer _overlayRenderer;
    private readonly MapDataService _mapDataService;
    private readonly IGameLifecycle _gameLifecycle;
    private readonly IGameAttachment _gameAttachment;
    private readonly IStatusToastPublisher _statusToasts;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<RuntimeOrchestratorHostedService> _logger;

    public RuntimeOrchestratorHostedService(
        IRuntimeOrchestrator orchestrator,
        IOffsetProvider offsetProvider,
        IOverlayRenderer overlayRenderer,
        MapDataService mapDataService,
        IGameLifecycle gameLifecycle,
        IGameAttachment gameAttachment,
        IStatusToastPublisher statusToasts,
        IHostApplicationLifetime lifetime,
        ILogger<RuntimeOrchestratorHostedService> logger)
    {
        _orchestrator = orchestrator;
        _offsetProvider = offsetProvider;
        _overlayRenderer = overlayRenderer;
        _mapDataService = mapDataService;
        _gameLifecycle = gameLifecycle;
        _gameAttachment = gameAttachment;
        _statusToasts = statusToasts;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Runtime orchestrator starting");

            await _offsetProvider.EnsureLoadedAsync(stoppingToken);
            _orchestrator.CompletePhase(StartupPhase.Offsets);

            await WaitForOverlayReadyAsync(stoppingToken);
            _orchestrator.CompletePhase(StartupPhase.Overlay);

            _statusToasts.Publish("Parsing maps...", TimeSpan.FromSeconds(8));
            await _mapDataService.ParseAllMapsAsync(
                message => _statusToasts.Publish(message, TimeSpan.FromSeconds(4)),
                stoppingToken);
            _orchestrator.CompletePhase(StartupPhase.Maps);

            _orchestrator.CompletePhase(StartupPhase.Input);
            _statusToasts.SetPersistent("Press inject key to attach to CS2");

            await WaitForAttachAsync(stoppingToken);
            _orchestrator.CompletePhase(StartupPhase.Attach);
            _statusToasts.ClearPersistent();

            _logger.LogInformation("Attach complete — game loop and features unblocked");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
        catch (Exception ex)
        {
            _orchestrator.Fail(ex);
            Environment.ExitCode = 1;
            _lifetime.StopApplication();
        }
    }

    private async Task WaitForOverlayReadyAsync(CancellationToken stoppingToken)
    {
        while (!_overlayRenderer.IsReady && !stoppingToken.IsCancellationRequested)
            await Task.Delay(50, stoppingToken);
    }

    private async Task WaitForAttachAsync(CancellationToken stoppingToken)
    {
        if (_gameAttachment.IsAttached)
            return;

        var attachTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnStateChanged(GameLifecycleState state)
        {
            if (state == GameLifecycleState.Attached)
                attachTcs.TrySetResult();
        }

        _gameLifecycle.StateChanged += OnStateChanged;
        try
        {
            if (_gameLifecycle.State == GameLifecycleState.Attached)
                return;

            using var registration = stoppingToken.Register(() => attachTcs.TrySetCanceled(stoppingToken));
            await attachTcs.Task;
        }
        finally
        {
            _gameLifecycle.StateChanged -= OnStateChanged;
        }
    }
}
