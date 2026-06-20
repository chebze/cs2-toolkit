using Cs2Toolkit.Configuration;

namespace Cs2Toolkit.Services;

public sealed class TbState
{
    private readonly object _lock = new();
    private int _enabled;
    private int _autoStopEnabled;
    private float _preFireFovDegrees = 0.7f;
    private float _minPreFireFovDegrees = 0.1f;
    private float _maxPreFireFovDegrees = 5f;
    private int _minReactionDelayMs = 200;
    private int _maxReactionDelayMs = 400;

    public bool IsEnabled => Volatile.Read(ref _enabled) == 1;

    public bool IsAutoStopEnabled => Volatile.Read(ref _autoStopEnabled) == 1;

    public float PreFireFovDegrees
    {
        get
        {
            lock (_lock)
                return _preFireFovDegrees;
        }
    }

    public int MinReactionDelayMs
    {
        get
        {
            lock (_lock)
                return _minReactionDelayMs;
        }
    }

    public int MaxReactionDelayMs
    {
        get
        {
            lock (_lock)
                return _maxReactionDelayMs;
        }
    }

    public void Initialize(TbOptions options)
    {
        Volatile.Write(ref _enabled, options.Enabled ? 1 : 0);
        Volatile.Write(ref _autoStopEnabled, options.AutoStopEnabled ? 1 : 0);

        lock (_lock)
        {
            _minPreFireFovDegrees = options.MinPreFireFovDegrees;
            _maxPreFireFovDegrees = options.MaxPreFireFovDegrees;
            _preFireFovDegrees = ClampFov(options.PreFireFovDegrees);
            _minReactionDelayMs = Math.Max(0, options.MinReactionDelayMs);
            _maxReactionDelayMs = Math.Max(50, options.MaxReactionDelayMs);
            if (_minReactionDelayMs > _maxReactionDelayMs)
                _minReactionDelayMs = _maxReactionDelayMs;
        }
    }

    public bool Toggle()
    {
        while (true)
        {
            var current = Volatile.Read(ref _enabled);
            var next = current == 1 ? 0 : 1;
            if (Interlocked.CompareExchange(ref _enabled, next, current) == current)
                return next == 1;
        }
    }

    public bool ToggleAutoStop()
    {
        while (true)
        {
            var current = Volatile.Read(ref _autoStopEnabled);
            var next = current == 1 ? 0 : 1;
            if (Interlocked.CompareExchange(ref _autoStopEnabled, next, current) == current)
                return next == 1;
        }
    }

    public void AdjustPreFireFovDegrees(float delta)
    {
        lock (_lock)
            _preFireFovDegrees = ClampFov(_preFireFovDegrees + delta);
    }

    public void AdjustReactionDelays(int deltaMs)
    {
        lock (_lock)
        {
            _minReactionDelayMs = Math.Max(0, _minReactionDelayMs + deltaMs);
            _maxReactionDelayMs = Math.Max(50, _maxReactionDelayMs + deltaMs);
            if (_minReactionDelayMs > _maxReactionDelayMs)
                _minReactionDelayMs = _maxReactionDelayMs;
        }
    }

    private float ClampFov(float value) =>
        Math.Clamp(value, _minPreFireFovDegrees, _maxPreFireFovDegrees);
}
