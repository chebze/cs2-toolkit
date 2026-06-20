using Cs2Toolkit.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Services;

public sealed class RcsToggleService : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly RcsState _rcsState;
    private readonly GlobalKeybindState _keybinds;
    private readonly ILogger<RcsToggleService> _logger;

    public RcsToggleService(
        ToolkitEventBus eventBus,
        RcsState rcsState,
        GlobalKeybindState keybinds,
        ILogger<RcsToggleService> logger)
    {
        _eventBus = eventBus;
        _rcsState = rcsState;
        _keybinds = keybinds;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress += OnKeyPress;
        _logger.LogInformation(
            "RCS toggle bound to {ToggleKey} (starts {State})",
            _keybinds.Current.RcsToggleKey,
            _rcsState.IsEnabled ? "enabled" : "disabled");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress -= OnKeyPress;
        return Task.CompletedTask;
    }

    private void OnKeyPress(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _keybinds.ParseToggleKey(k => k.RcsToggleKey))
            return;

        var enabled = _rcsState.Toggle();
        _logger.LogInformation("RCS {State}", enabled ? "enabled" : "disabled");
    }
}
