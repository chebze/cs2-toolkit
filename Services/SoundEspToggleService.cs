using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class SoundEspToggleService : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly SoundEspState _soundEspState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<SoundEspToggleService> _logger;
    private Keys _toggleKey = Keys.F5;

    public SoundEspToggleService(
        ToolkitEventBus eventBus,
        SoundEspState soundEspState,
        IOptions<ToolkitOptions> options,
        ILogger<SoundEspToggleService> logger)
    {
        _eventBus = eventBus;
        _soundEspState = soundEspState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _toggleKey = KeyParser.Parse(_options.SoundEsp.ToggleKey);
        if (_toggleKey == Keys.None)
            throw new InvalidOperationException($"Invalid SoundEsp:ToggleKey in appsettings.json: {_options.SoundEsp.ToggleKey}");

        _soundEspState.Initialize(_options.SoundEsp);
        _eventBus.OnKeyPress += OnKeyPress;
        _logger.LogInformation(
            "Sound ESP toggle bound to {ToggleKey} (enemy noise + bomb waves, starts {State})",
            _options.SoundEsp.ToggleKey,
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
        if (e.Key != _toggleKey)
            return;

        var enabled = _soundEspState.Toggle();
        _logger.LogInformation("S-ESP {State}", enabled ? "enabled" : "disabled");
    }
}
