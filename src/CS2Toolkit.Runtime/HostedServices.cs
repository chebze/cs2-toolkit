using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Game.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Runtime;

internal sealed class InjectKeybindOrchestrator : IHostedService
{
    private readonly IKeybindDispatcher _keybindDispatcher;
    private readonly IGameAttachment _gameAttachment;
    private readonly IStatusToastPublisher _statusToasts;
    private readonly ILogger<InjectKeybindOrchestrator> _logger;

    public InjectKeybindOrchestrator(
        IKeybindDispatcher keybindDispatcher,
        IGameAttachment gameAttachment,
        IStatusToastPublisher statusToasts,
        ILogger<InjectKeybindOrchestrator> logger)
    {
        _keybindDispatcher = keybindDispatcher;
        _gameAttachment = gameAttachment;
        _statusToasts = statusToasts;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _keybindDispatcher.KeybindActivated += OnKeybindActivated;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _keybindDispatcher.KeybindActivated -= OnKeybindActivated;
        return Task.CompletedTask;
    }

    private void OnKeybindActivated(object? sender, KeybindActivatedEventArgs e)
    {
        if (e.Match.ActionId != ToolkitKeybindActions.Inject)
            return;

        if (_gameAttachment.IsAttached)
        {
            _logger.LogInformation("Already attached to CS2");
            _statusToasts.Publish("Already attached to CS2", TimeSpan.FromSeconds(2));
            return;
        }

        _statusToasts.Publish("Attaching to CS2...", TimeSpan.FromSeconds(5));
        if (_gameAttachment.TryAttach("cs2"))
        {
            _statusToasts.ClearPersistent();
            _statusToasts.Publish("Attached to CS2", TimeSpan.FromSeconds(2));
            return;
        }

        _statusToasts.Publish("Failed to attach to CS2", TimeSpan.FromSeconds(4), 0xFFFF6B6B);
    }
}

internal sealed class StartupLoggerHostedService(
    ILogger<StartupLoggerHostedService> logger,
    IActiveConfiguration configuration) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CS2 Toolkit v2 ready");
        logger.LogInformation(
            "Active profile: {ProfileName} ({ProfileId}), web port {WebPort}",
            configuration.Current.ActiveProfileName,
            configuration.Current.ActiveProfileId,
            configuration.Current.WebPort);

        var ak = configuration.ResolveWeapon(7);
        logger.LogInformation(
            "Resolved AK-47 triggerbot enabled={Enabled}, preFireFov={Fov}",
            ak.Triggerbot.Enabled,
            ak.Triggerbot.PreFireFovDegrees);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
