using Cs2Toolkit.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Services;

public sealed class SoundEspToggleService : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly SoundEspState _soundEspState;
    private readonly GlobalKeybindState _keybinds;
    private readonly ILogger<SoundEspToggleService> _logger;

    public SoundEspToggleService(
        ToolkitEventBus eventBus,
        SoundEspState soundEspState,
        GlobalKeybindState keybinds,
        ILogger<SoundEspToggleService> logger)
    {
        _eventBus = eventBus;
        _soundEspState = soundEspState;
        _keybinds = keybinds;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress += OnKeyPress;
        _logger.LogInformation(
            "Sound ESP toggle bound to {ToggleKey} (enemy noise + bomb waves, starts {State})",
            _keybinds.Current.SoundEspToggleKey,
            _soundEspState.IsEnabled ? "enabled" : "disabled");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress -= OnKeyPress;
        return Task.CompletedTask;
    }

    private void OnKeyPress(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _keybinds.ParseToggleKey(k => k.SoundEspToggleKey))
            return;

        var enabled = _soundEspState.Toggle();
        _logger.LogInformation("S-ESP {State}", enabled ? "enabled" : "disabled");
    }
}
