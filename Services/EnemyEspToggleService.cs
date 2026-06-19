using Cs2Toolkit.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Services;

public sealed class EnemyEspToggleService : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly EnemyEspState _espState;
    private readonly GlobalKeybindState _keybinds;
    private readonly ILogger<EnemyEspToggleService> _logger;

    public EnemyEspToggleService(
        ToolkitEventBus eventBus,
        EnemyEspState espState,
        GlobalKeybindState keybinds,
        ILogger<EnemyEspToggleService> logger)
    {
        _eventBus = eventBus;
        _espState = espState;
        _keybinds = keybinds;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress += OnKeyPress;
        _logger.LogInformation(
            "Enemy ESP toggle bound to {ToggleKey} (cycles disabled → last seen → full, starts {Mode})",
            _keybinds.Current.EnemyEspToggleKey,
            _espState.Mode);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnKeyPress -= OnKeyPress;
        return Task.CompletedTask;
    }

    private void OnKeyPress(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _keybinds.ParseToggleKey(k => k.EnemyEspToggleKey))
            return;

        var mode = _espState.CycleMode();
        _logger.LogInformation("Enemy ESP mode set to {Mode}", mode);
    }
}
