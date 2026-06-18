using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class EnemyEspToggleService : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly EnemyEspState _espState;
    private readonly ToolkitOptions _options;
    private readonly ILogger<EnemyEspToggleService> _logger;
    private Keys _toggleKey = Keys.F6;

    public EnemyEspToggleService(
        ToolkitEventBus eventBus,
        EnemyEspState espState,
        IOptions<ToolkitOptions> options,
        ILogger<EnemyEspToggleService> logger)
    {
        _eventBus = eventBus;
        _espState = espState;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _toggleKey = KeyParser.Parse(_options.EnemyEsp.ToggleKey);
        if (_toggleKey == Keys.None)
            throw new InvalidOperationException($"Invalid EnemyEsp:ToggleKey in appsettings.json: {_options.EnemyEsp.ToggleKey}");

        _espState.Initialize(_options.EnemyEsp);
        _eventBus.OnKeyPress += OnKeyPress;
        _logger.LogInformation(
            "Enemy ESP toggle bound to {ToggleKey} (cycles disabled → last seen → full, starts {Mode})",
            _options.EnemyEsp.ToggleKey,
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
        if (e.Key != _toggleKey)
            return;

        var mode = _espState.CycleMode();
        _logger.LogInformation("Enemy ESP mode set to {Mode}", mode);
    }
}
