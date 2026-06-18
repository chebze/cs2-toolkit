using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class TbToggleService : BackgroundService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly TbState _tbState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<TbToggleService> _logger;
    private Keys _toggleKey = Keys.F7;
    private Keys _autoStrafeKey = Keys.Space;
    private float _fovStep = 0.05f;
    private int _fovRepeatIntervalMs = 80;
    private int _reactionDelayStepMs = 50;
    private bool _settingsAdjustedThisHold;

    public TbToggleService(
        ToolkitEventBus eventBus,
        TbState tbState,
        IOptions<ToolkitOptions> options,
        ILogger<TbToggleService> logger)
    {
        _eventBus = eventBus;
        _tbState = tbState;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _toggleKey = KeyParser.Parse(_options.Tb.ToggleKey);
        if (_toggleKey == Keys.None)
            throw new InvalidOperationException($"Invalid Tb:ToggleKey in appsettings.json: {_options.Tb.ToggleKey}");

        _autoStrafeKey = KeyParser.Parse(_options.Tb.AutoStrafeKey);
        if (_autoStrafeKey == Keys.None)
            throw new InvalidOperationException($"Invalid Tb:AutoStrafeKey in appsettings.json: {_options.Tb.AutoStrafeKey}");

        _fovStep = _options.Tb.FovAdjustStepDegrees;
        _fovRepeatIntervalMs = _options.Tb.FovAdjustRepeatIntervalMs;
        _reactionDelayStepMs = _options.Tb.ReactionDelayAdjustStepMs;
        _tbState.Initialize(_options.Tb);

        _eventBus.OnKeyDown += OnKeyDown;
        _eventBus.OnKeyUp += OnKeyUp;

        try
        {
            _logger.LogInformation(
                "TB toggle bound to {ToggleKey} (tap to toggle, hold + arrows to adjust FOV/reaction delay, starts {State})",
                _options.Tb.ToggleKey,
                _tbState.IsEnabled ? "enabled" : "disabled");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (NativeInput.IsKeyDown(_toggleKey))
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
        if (e.Key == _toggleKey)
        {
            _settingsAdjustedThisHold = false;
            return;
        }

        if (!NativeInput.IsKeyDown(_toggleKey))
            return;

        if (e.Key == _autoStrafeKey)
        {
            var enabled = _tbState.ToggleAutoStop();
            _settingsAdjustedThisHold = true;
            _logger.LogInformation("TB auto-stop {State}", enabled ? "enabled" : "disabled");
        }
    }

    private void OnKeyUp(object? sender, KeyInputEventArgs e)
    {
        if (e.Key != _toggleKey)
            return;

        if (_settingsAdjustedThisHold)
            return;

        var enabled = _tbState.Toggle();
        _logger.LogInformation("TB {State}", enabled ? "enabled" : "disabled");
    }

    private async Task AdjustSettingsWhileHeldAsync(CancellationToken stoppingToken)
    {
        var left = NativeInput.IsKeyDown(Keys.Left);
        var right = NativeInput.IsKeyDown(Keys.Right);
        var up = NativeInput.IsKeyDown(Keys.Up);
        var down = NativeInput.IsKeyDown(Keys.Down);

        if (left != right)
        {
            AdjustFov(right ? _fovStep : -_fovStep);
            await Task.Delay(_fovRepeatIntervalMs, stoppingToken);
            return;
        }

        if (up != down)
        {
            AdjustReactionDelays(up ? _reactionDelayStepMs : -_reactionDelayStepMs);
            await Task.Delay(_fovRepeatIntervalMs, stoppingToken);
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
