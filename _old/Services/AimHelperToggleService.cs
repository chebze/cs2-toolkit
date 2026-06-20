using Cs2Toolkit.Events;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class AimHelperToggleService : BackgroundService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly AimHelperState _aimHelperState;
    private readonly GlobalKeybindState _keybinds;
    private readonly RuntimeConfigProvider _runtimeConfig;
    private readonly ILogger<AimHelperToggleService> _logger;
    private bool _settingsAdjustedThisHold;

    public AimHelperToggleService(
        ToolkitEventBus eventBus,
        AimHelperState aimHelperState,
        GlobalKeybindState keybinds,
        RuntimeConfigProvider runtimeConfig,
        ILogger<AimHelperToggleService> logger)
    {
        _eventBus = eventBus;
        _aimHelperState = aimHelperState;
        _keybinds = keybinds;
        _runtimeConfig = runtimeConfig;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _eventBus.OnKeyDown += OnKeyDown;
        _eventBus.OnKeyUp += OnKeyUp;

        try
        {
            _logger.LogInformation(
                "Aim helper toggle bound to {ToggleKey} (tap to toggle, hold + arrows to adjust FOV, starts {State})",
                _keybinds.Current.AimHelperToggleKey,
                _aimHelperState.IsEnabled ? "enabled" : "disabled");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (NativeInput.IsKeyDown(_keybinds.ParseToggleKey(k => k.AimHelperToggleKey)))
                    await AdjustFovWhileHeldAsync(stoppingToken);

                await Task.Delay(16, stoppingToken);
            }
        }
        finally
        {
            _eventBus.OnKeyDown -= OnKeyDown;
            _eventBus.OnKeyUp -= OnKeyUp;
        }
    }

    private void OnKeyDown(object? sender, KeyInputEventArgs e)
    {
        if (e.Key == _keybinds.ParseToggleKey(k => k.AimHelperToggleKey))
            _settingsAdjustedThisHold = false;
    }

    private void OnKeyUp(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _keybinds.ParseToggleKey(k => k.AimHelperToggleKey))
            return;

        if (_settingsAdjustedThisHold)
            return;

        var enabled = _aimHelperState.Toggle();
        _logger.LogInformation("Aim helper {State}", enabled ? "enabled" : "disabled");
    }

    private async Task AdjustFovWhileHeldAsync(CancellationToken stoppingToken)
    {
        var aim = _runtimeConfig.Current.AimHelper;
        var left = NativeInput.IsKeyDown(Keys.Left);
        var right = NativeInput.IsKeyDown(Keys.Right);

        if (left == right)
            return;

        AdjustFov(right ? aim.FovAdjustStepDegrees : -aim.FovAdjustStepDegrees);
        await Task.Delay(aim.FovAdjustRepeatIntervalMs, stoppingToken);
    }

    private void AdjustFov(float delta)
    {
        var previous = _aimHelperState.FovDegrees;
        _aimHelperState.AdjustFovDegrees(delta);
        var current = _aimHelperState.FovDegrees;
        if (Math.Abs(current - previous) < 0.0001f)
            return;

        _settingsAdjustedThisHold = true;
        _logger.LogInformation("Aim helper FOV set to {Fov:F2}°", current);
    }
}
