using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Runtime.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Input;

public sealed class KeybindDispatcher : BackgroundService, IKeybindDispatcher
{
    private readonly IInputListener _inputListener;
    private readonly IKeybindMatcher _keybindMatcher;
    private readonly IRuntimeOrchestrator _orchestrator;
    private readonly ILogger<KeybindDispatcher> _logger;

    public KeybindDispatcher(
        IInputListener inputListener,
        IKeybindMatcher keybindMatcher,
        IRuntimeOrchestrator orchestrator,
        ILogger<KeybindDispatcher> logger)
    {
        _inputListener = inputListener;
        _keybindMatcher = keybindMatcher;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public event EventHandler<KeybindActivatedEventArgs>? KeybindActivated;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _orchestrator.WaitForPhaseAsync(StartupPhase.Overlay, stoppingToken);
        _inputListener.KeyDown += OnKeyDown;
        _logger.LogInformation(
            "Keybind dispatcher ready ({Count} bindings)",
            _keybindMatcher.GetKeybinds().Count(k => !string.IsNullOrWhiteSpace(k.KeyName)));

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
        finally
        {
            _inputListener.KeyDown -= OnKeyDown;
        }
    }

    private void OnKeyDown(object? sender, KeyInputEvent e)
    {
        if (!_keybindMatcher.TryMatchKeyDown(e, out var match))
            return;

        _logger.LogInformation("Keybind activated: {ActionId} ({KeyName})", match.ActionId, match.KeyName);
        KeybindActivated?.Invoke(this, new KeybindActivatedEventArgs { Match = match });
    }
}
