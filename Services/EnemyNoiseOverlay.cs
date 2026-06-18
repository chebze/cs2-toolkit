using System.Drawing;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Models;
using Cs2Toolkit.Overlay;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class EnemyNoiseOverlay : IHostedService
{
    private readonly EnemySoundTracker _soundTracker;
    private readonly SoundEspState _soundEspState;
    private readonly ViewMatrixHolder _viewMatrixHolder;
    private readonly ToolkitEventBus _eventBus;
    private readonly ScreenOverlayManager _overlayManager;
    private readonly ToolkitOptions _options;
    private readonly ILogger<EnemyNoiseOverlay> _logger;
    private readonly object _lock = new();
    private readonly List<ActiveNoiseWave> _waves = [];

    private OverlayLayer? _layer;
    private Vector3? _bombPosition;
    private DateTime _bombWaveEpoch = DateTime.UtcNow;
    private bool _bombWasActive;

    public EnemyNoiseOverlay(
        EnemySoundTracker soundTracker,
        SoundEspState soundEspState,
        ViewMatrixHolder viewMatrixHolder,
        ToolkitEventBus eventBus,
        ScreenOverlayManager overlayManager,
        IOptions<ToolkitOptions> options,
        ILogger<EnemyNoiseOverlay> logger)
    {
        _soundTracker = soundTracker;
        _soundEspState = soundEspState;
        _viewMatrixHolder = viewMatrixHolder;
        _eventBus = eventBus;
        _overlayManager = overlayManager;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layer = _overlayManager.GetOrCreateLayer("enemy-noise", zIndex: 200);
        _soundTracker.OnEnemyNoise += OnEnemyNoise;
        _eventBus.OnMemoryRead += OnMemoryRead;
        _layer.QueueDraw(DrawWaves);
        _logger.LogInformation("EnemyNoiseOverlay subscribed to OnEnemyNoise");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _soundTracker.OnEnemyNoise -= OnEnemyNoise;
        _eventBus.OnMemoryRead -= OnMemoryRead;
        _layer?.ClearQueue();
        return Task.CompletedTask;
    }

    private void OnMemoryRead(object? sender, MemoryReadEventArgs e)
    {
        lock (_lock)
        {
            if (!_soundEspState.IsEnabled || !e.State.IsInMatch)
            {
                _waves.Clear();
                _bombPosition = null;
                _bombWasActive = false;
                return;
            }

            var bomb = e.State.Bomb;
            var hasBombWaves = bomb.WorldPosition is { IsValid: true }
                && bomb.Status is BombStatus.Planted or BombStatus.Defusing;

            if (!hasBombWaves)
            {
                _bombPosition = null;
                _bombWasActive = false;
                return;
            }

            if (!_bombWasActive)
                _bombWaveEpoch = DateTime.UtcNow;

            _bombPosition = bomb.WorldPosition;
            _bombWasActive = true;
        }
    }

    private void OnEnemyNoise(object? sender, EnemyNoiseEventArgs e)
    {
        if (!_soundEspState.IsEnabled)
            return;

        lock (_lock)
        {
            _waves.Add(new ActiveNoiseWave
            {
                WorldPosition = e.WorldPosition,
                StartedAt = e.Timestamp
            });
        }

        _overlayManager.EnsureOnTop();
    }

    private void DrawWaves(Graphics graphics)
    {
        if (!_soundEspState.IsEnabled)
            return;

        var noiseOptions = _options.EnemyNoise;
        var bounds = GameWindowHelper.GetTargetBounds();
        var now = DateTime.UtcNow;
        Span<float> viewMatrix = stackalloc float[16];
        _viewMatrixHolder.CopyTo(viewMatrix);
        var waveColor = DrawHelper.ParseColor(noiseOptions.WaveColor, Color.Red);
        Vector3? bombPosition = null;
        DateTime bombWaveEpoch;

        lock (_lock)
        {
            for (var i = _waves.Count - 1; i >= 0; i--)
            {
                var elapsedMs = (now - _waves[i].StartedAt).TotalMilliseconds;
                if (elapsedMs >= noiseOptions.WaveDurationMs)
                    _waves.RemoveAt(i);
            }

            foreach (var wave in _waves)
            {
                var progress = (float)Math.Clamp(
                    (now - wave.StartedAt).TotalMilliseconds / noiseOptions.WaveDurationMs,
                    0d,
                    1d);

                GroundWaveDrawer.DrawRings(
                    graphics,
                    wave.WorldPosition,
                    progress,
                    waveColor,
                    noiseOptions,
                    viewMatrix,
                    bounds.Width,
                    bounds.Height,
                    noiseOptions.WaveLineWidth);
            }

            bombPosition = _bombPosition;
            bombWaveEpoch = _bombWaveEpoch;
        }

        if (bombPosition is { IsValid: true } position)
        {
            var elapsedMs = (now - bombWaveEpoch).TotalMilliseconds;
            var progress = (float)(elapsedMs % noiseOptions.WaveDurationMs / noiseOptions.WaveDurationMs);

            GroundWaveDrawer.DrawRings(
                graphics,
                position,
                progress,
                waveColor,
                noiseOptions,
                viewMatrix,
                bounds.Width,
                bounds.Height,
                noiseOptions.WaveLineWidth);
        }
    }

    private sealed class ActiveNoiseWave
    {
        public Vector3 WorldPosition { get; init; }
        public DateTime StartedAt { get; init; }
    }
}
