using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class AimHelperToggleService : BackgroundService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly AimHelperState _aimHelperState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<AimHelperToggleService> _logger;
    private Keys _toggleKey = Keys.F4;
    private float _fovStep = 0.25f;
    private int _fovRepeatIntervalMs = 80;
    private bool _settingsAdjustedThisHold;

    public AimHelperToggleService(
        ToolkitEventBus eventBus,
        AimHelperState aimHelperState,
        IOptions<ToolkitOptions> options,
        ILogger<AimHelperToggleService> logger)
    {
        _eventBus = eventBus;
        _aimHelperState = aimHelperState;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _toggleKey = KeyParser.Parse(_options.AimHelper.ToggleKey);
        if (_toggleKey == Keys.None)
            throw new InvalidOperationException($"Invalid AimHelper:ToggleKey in appsettings.json: {_options.AimHelper.ToggleKey}");

        _fovStep = _options.AimHelper.FovAdjustStepDegrees;
        _fovRepeatIntervalMs = _options.AimHelper.FovAdjustRepeatIntervalMs;
        _aimHelperState.Initialize(_options.AimHelper);

        _eventBus.OnKeyDown += OnKeyDown;
        _eventBus.OnKeyUp += OnKeyUp;

        try
        {
            _logger.LogInformation(
                "Aim helper toggle bound to {ToggleKey} (tap to toggle, hold + arrows to adjust FOV, starts {State})",
                _options.AimHelper.ToggleKey,
                _aimHelperState.IsEnabled ? "enabled" : "disabled");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (NativeInput.IsKeyDown(_toggleKey))
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
        if (e.Key == _toggleKey)
            _settingsAdjustedThisHold = false;
    }

    private void OnKeyUp(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _toggleKey)
            return;

        if (_settingsAdjustedThisHold)
            return;

        var enabled = _aimHelperState.Toggle();
        _logger.LogInformation("Aim helper {State}", enabled ? "enabled" : "disabled");
    }

    private async Task AdjustFovWhileHeldAsync(CancellationToken stoppingToken)
    {
        var left = NativeInput.IsKeyDown(Keys.Left);
        var right = NativeInput.IsKeyDown(Keys.Right);

        if (left == right)
            return;

        AdjustFov(right ? _fovStep : -_fovStep);
        await Task.Delay(_fovRepeatIntervalMs, stoppingToken);
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
