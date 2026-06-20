using CS2Toolkit.Input.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Input;

public sealed class KeybindDispatcher : IHostedService, IKeybindDispatcher
{
    private readonly IInputListener _inputListener;
    private readonly IKeybindMatcher _keybindMatcher;
    private readonly ILogger<KeybindDispatcher> _logger;

    public KeybindDispatcher(
        IInputListener inputListener,
        IKeybindMatcher keybindMatcher,
        ILogger<KeybindDispatcher> logger)
    {
        _inputListener = inputListener;
        _keybindMatcher = keybindMatcher;
        _logger = logger;
    }

    public event EventHandler<KeybindActivatedEventArgs>? KeybindActivated;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _inputListener.KeyDown += OnKeyDown;
        _logger.LogInformation(
            "Keybind dispatcher ready ({Count} bindings)",
            _keybindMatcher.GetKeybinds().Count(k => !string.IsNullOrWhiteSpace(k.KeyName)));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _inputListener.KeyDown -= OnKeyDown;
        return Task.CompletedTask;
    }

    private void OnKeyDown(object? sender, KeyInputEvent e)
    {
        if (!_keybindMatcher.TryMatchKeyDown(e, out var match))
            return;

        _logger.LogInformation("Keybind activated: {ActionId} ({KeyName})", match.ActionId, match.KeyName);
        KeybindActivated?.Invoke(this, new KeybindActivatedEventArgs { Match = match });
    }
}
