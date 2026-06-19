using Cs2Toolkit.Events;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class TbToggleService : BackgroundService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly TbState _tbState;
    private readonly GlobalKeybindState _keybinds;
    private readonly RuntimeConfigProvider _runtimeConfig;
    private readonly ILogger<TbToggleService> _logger;
    private bool _settingsAdjustedThisHold;

    public TbToggleService(
        ToolkitEventBus eventBus,
        TbState tbState,
        GlobalKeybindState keybinds,
        RuntimeConfigProvider runtimeConfig,
        ILogger<TbToggleService> logger)
    {
        _eventBus = eventBus;
        _tbState = tbState;
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
                "TB toggle bound to {ToggleKey} (tap to toggle, hold + arrows to adjust FOV/reaction delay, starts {State})",
                _keybinds.Current.TbToggleKey,
                _tbState.IsEnabled ? "enabled" : "disabled");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (NativeInput.IsKeyDown(_keybinds.ParseToggleKey(k => k.TbToggleKey)))
                    await AdjustSettingsWhileHeldAsync(stoppingToken);

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
        var toggleKey = _keybinds.ParseToggleKey(k => k.TbToggleKey);
        if (e.Key == toggleKey)
        {
            _settingsAdjustedThisHold = false;
            return;
        }

        if (!NativeInput.IsKeyDown(toggleKey))
            return;

        if (e.Key == _keybinds.ParseToggleKey(k => k.TbAutoStrafeKey))
        {
            var enabled = _tbState.ToggleAutoStop();
            _settingsAdjustedThisHold = true;
            _logger.LogInformation("TB auto-stop {State}", enabled ? "enabled" : "disabled");
        }
    }

    private void OnKeyUp(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _keybinds.ParseToggleKey(k => k.TbToggleKey))
            return;

        if (_settingsAdjustedThisHold)
            return;

        var enabled = _tbState.Toggle();
        _logger.LogInformation("TB {State}", enabled ? "enabled" : "disabled");
    }

    private async Task AdjustSettingsWhileHeldAsync(CancellationToken stoppingToken)
    {
        var tb = _runtimeConfig.Current.Tb;
        var left = NativeInput.IsKeyDown(Keys.Left);
        var right = NativeInput.IsKeyDown(Keys.Right);
        var up = NativeInput.IsKeyDown(Keys.Up);
        var down = NativeInput.IsKeyDown(Keys.Down);

        if (left != right)
        {
            AdjustFov(right ? tb.FovAdjustStepDegrees : -tb.FovAdjustStepDegrees);
            await Task.Delay(tb.FovAdjustRepeatIntervalMs, stoppingToken);
            return;
        }

        if (up != down)
        {
            AdjustReactionDelays(up ? tb.ReactionDelayAdjustStepMs : -tb.ReactionDelayAdjustStepMs);
            await Task.Delay(tb.FovAdjustRepeatIntervalMs, stoppingToken);
        }
    }

    private void AdjustFov(float delta)
    {
        var previous = _tbState.PreFireFovDegrees;
        _tbState.AdjustPreFireFovDegrees(delta);
        var current = _tbState.PreFireFovDegrees;
        if (Math.Abs(current - previous) < 0.0001f)
            return;

        _settingsAdjustedThisHold = true;
        _logger.LogInformation("TB FOV set to {Fov:F2}°", current);
    }

    private void AdjustReactionDelays(int deltaMs)
    {
        var previousMin = _tbState.MinReactionDelayMs;
        var previousMax = _tbState.MaxReactionDelayMs;
        _tbState.AdjustReactionDelays(deltaMs);
        var currentMin = _tbState.MinReactionDelayMs;
        var currentMax = _tbState.MaxReactionDelayMs;
        if (currentMin == previousMin && currentMax == previousMax)
            return;

        _settingsAdjustedThisHold = true;
        _logger.LogInformation("TB reaction delay set to {MinMs}-{MaxMs} ms", currentMin, currentMax);
    }
}
