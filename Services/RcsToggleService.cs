using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class RcsToggleService : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly RcsState _rcsState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<RcsToggleService> _logger;
    private Keys _toggleKey = Keys.F8;

    public RcsToggleService(
        ToolkitEventBus eventBus,
        RcsState rcsState,
        IOptions<ToolkitOptions> options,
        ILogger<RcsToggleService> logger)
    {
        _eventBus = eventBus;
        _rcsState = rcsState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _toggleKey = KeyParser.Parse(_options.Rcs.ToggleKey);
        if (_toggleKey == Keys.None)
            throw new InvalidOperationException($"Invalid Rcs:ToggleKey in appsettings.json: {_options.Rcs.ToggleKey}");

        _eventBus.OnKeyPress += OnKeyPress;
        _logger.LogInformation("RCS toggle bound to {ToggleKey} (starts disabled)", _options.Rcs.ToggleKey);
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

        var enabled = _rcsState.Toggle();
        _logger.LogInformation("RCS {State}", enabled ? "enabled" : "disabled");
    }
}
