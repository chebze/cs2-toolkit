using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Offsets;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Runtime;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class ToolkitRuntime : BackgroundService
{
    private readonly OffsetDownloader _offsetDownloader;
    private readonly ProcessMemory _processMemory;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly RuntimeGate _runtimeGate;
    private readonly ToolkitEventBus _eventBus;
    private readonly MapDataService _mapDataService;
    private readonly ToolkitOptions _options;
    private readonly ILogger<ToolkitRuntime> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly HashSet<Keys> _heldKeys = new();
    private readonly HashSet<MouseButtons> _heldMouseButtons = new();
    private OverlayLayer? _systemLayer;
    private Keys _panicKey = Keys.F10;

    public ToolkitRuntime(
        OffsetDownloader offsetDownloader,
        ProcessMemory processMemory,
        ScreenOverlayManager overlayManager,
        RuntimeGate runtimeGate,
        ToolkitEventBus eventBus,
        MapDataService mapDataService,
        IOptions<ToolkitOptions> options,
        ILogger<ToolkitRuntime> logger,
        IHostApplicationLifetime lifetime)
    {
        _offsetDownloader = offsetDownloader;
        _processMemory = processMemory;
        _overlayManager = overlayManager;
        _runtimeGate = runtimeGate;
        _eventBus = eventBus;
        _mapDataService = mapDataService;
        _options = options.Value;
        _logger = logger;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogCritical(ex, "ToolkitRuntime failed");
            Environment.ExitCode = 1;
            _lifetime.StopApplication();
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ToolkitRuntime starting");

        await _offsetDownloader.DownloadAsync(stoppingToken);

        await _overlayManager.StartAsync(stoppingToken);
        _systemLayer = _overlayManager.GetOrCreateLayer("system", zIndex: 1000);
        _panicKey = ParseRequiredKey(_options.PanicKey, nameof(_options.PanicKey));

        QueueSystemMessage("Parsing maps...");
        await _mapDataService.ParseAllMapsAsync(QueueSystemMessage, stoppingToken);

        _runtimeGate.SignalOverlayReady();
        _logger.LogInformation("Screen overlay ready");

        _eventBus.OnMemoryRead += OnMemoryRead;
        await RunInjectionFlowAsync(stoppingToken);
        await RunInputEventLoopAsync(stoppingToken);
    }

    private async Task RunInjectionFlowAsync(CancellationToken stoppingToken)
    {
        _eventBus.PublishInjectionStatus(InjectionStatus.WaitingForGame, "Waiting for CS2...");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (TryHandlePanicKey())
                return;

            var cs2Running = System.Diagnostics.Process.GetProcessesByName("cs2").Length > 0;
            if (cs2Running)
                break;

            QueueSystemMessage("Waiting for CS2 to start...");
            await Task.Delay(500, stoppingToken);
        }

        var injectKey = ParseRequiredKey(_options.InjectKey, nameof(_options.InjectKey));

        _eventBus.PublishInjectionStatus(InjectionStatus.WaitingForKeyPress, $"Press {_options.InjectKey} to inject");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (TryHandlePanicKey())
                return;

            QueueInjectionPrompt($"Press {_options.InjectKey} to inject...");

            if (NativeInput.IsKeyDown(injectKey))
            {
                await PerformInjectionAsync(stoppingToken);
                break;
            }

            await Task.Delay(50, stoppingToken);
        }
    }

    private async Task PerformInjectionAsync(CancellationToken stoppingToken)
    {
        _eventBus.PublishInjectionStatus(InjectionStatus.Attaching, "Attaching to CS2...");
        QueueSystemMessage("Injection status: Attaching...");

        await Task.Delay(100, stoppingToken);

        var attached = _processMemory.AttachToProcess("cs2");
        if (!attached)
        {
            _eventBus.PublishInjectionStatus(InjectionStatus.Failed, "Failed to attach to CS2");
            QueueSystemMessage("Injection status: Failed");
            throw new InvalidOperationException("Failed to attach to CS2 process. Ensure the game is running.");
        }

        _eventBus.PublishInjectionStatus(InjectionStatus.Attached, "Attached to CS2");
        _systemLayer?.ClearQueue();
        _overlayManager.EnsureOnTop();
        _runtimeGate.SignalInjectionComplete();
        _logger.LogInformation("Injection complete — memory reader may start");
    }

    private async Task RunInputEventLoopAsync(CancellationToken stoppingToken)
    {
        var previousMousePosition = NativeInput.GetCursorPosition();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (TryHandlePanicKey())
                return;

            PollKeyboard();
            PollMouse(ref previousMousePosition);
            await Task.Delay(16, stoppingToken);
        }
    }

    private void PollKeyboard()
    {
        foreach (Keys key in Enum.GetValues<Keys>())
        {
            if (key is Keys.None or Keys.LButton or Keys.RButton or Keys.MButton or Keys.XButton1 or Keys.XButton2)
                continue;

            var isDown = NativeInput.IsKeyDown(key);

            if (isDown && !_heldKeys.Contains(key))
            {
                _heldKeys.Add(key);
                var args = new KeyInputEventArgs { Key = key };
                _eventBus.PublishKeyDown(args);
                _eventBus.PublishKeyPress(args);
            }
            else if (!isDown && _heldKeys.Contains(key))
            {
                _heldKeys.Remove(key);
                _eventBus.PublishKeyUp(new KeyInputEventArgs { Key = key });
            }
        }
    }

    private void PollMouse(ref (int X, int Y) previousPosition)
    {
        var position = NativeInput.GetCursorPosition();
        if (position != previousPosition)
        {
            _eventBus.PublishMouseMove(new MouseInputEventArgs
            {
                X = position.X,
                Y = position.Y,
                Button = MouseButtons.None
            });
            previousPosition = position;
        }

        var pressed = NativeInput.GetPressedMouseButtons();
        foreach (MouseButtons button in Enum.GetValues<MouseButtons>())
        {
            if (button == MouseButtons.None)
                continue;

            var isPressed = pressed.HasFlag(button);
            if (isPressed && !_heldMouseButtons.Contains(button))
            {
                _heldMouseButtons.Add(button);
                _eventBus.PublishMousePress(new MouseInputEventArgs
                {
                    Button = button,
                    X = position.X,
                    Y = position.Y
                });
            }
            else if (!isPressed)
            {
                _heldMouseButtons.Remove(button);
            }
        }
    }

    private void OnMemoryRead(object? sender, MemoryReadEventArgs e)
    {
        if (!_runtimeGate.IsInjectionComplete || e.State.IsInMatch)
            return;

        _systemLayer?.ClearQueue();
    }

    private bool TryHandlePanicKey()
    {
        if (!NativeInput.IsKeyDown(_panicKey))
            return false;

        _logger.LogInformation("Panic key ({PanicKey}) pressed — shutting down", _options.PanicKey);
        _processMemory.Detach();
        _overlayManager.Dispose();
        _lifetime.StopApplication();
        return true;
    }

    private static Keys ParseRequiredKey(string keyName, string optionName)
    {
        var key = KeyParser.Parse(keyName);
        if (key == Keys.None)
            throw new InvalidOperationException($"Invalid {optionName} in appsettings.json: {keyName}");

        return key;
    }

    private void QueueInjectionPrompt(string message)
    {
        if (_systemLayer is null)
            return;

        _overlayManager.EnsureOnTop();

        var color = DrawHelper.ParseColor(_options.Overlay.InjectionPrompt.Color, Color.White);
        var fontSize = _options.Overlay.InjectionPrompt.FontSize;

        _systemLayer.QueueDraw(g => DrawHelper.DrawTextTopRight(g, message, color, fontSize));
    }

    private void QueueSystemMessage(string message)
    {
        if (_systemLayer is null)
            return;

        _overlayManager.EnsureOnTop();

        var color = DrawHelper.ParseColor(_options.Overlay.InjectionPrompt.Color, Color.White);
        var fontSize = _options.Overlay.InjectionPrompt.FontSize;

        _systemLayer.QueueDraw(g => DrawHelper.DrawTextTopRight(g, message, color, fontSize));
    }
}
