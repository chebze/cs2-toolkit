using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Services;

public sealed class AimHelperState
{
    private readonly object _lock = new();
    private int _enabled;
    private float _fovDegrees = 3f;
    private float _minFovDegrees = 0.5f;
    private float _maxFovDegrees = 15f;
    private AimHelperBone _preferredBone = AimHelperBone.Head;

    public bool IsEnabled => Volatile.Read(ref _enabled) == 1;

    public float FovDegrees
    {
        get
        {
            lock (_lock)
                return _fovDegrees;
        }
    }

    public AimHelperBone PreferredBone
    {
        get
        {
            lock (_lock)
                return _preferredBone;
        }
    }

    public void Initialize(AimHelperOptions options)
    {
        Volatile.Write(ref _enabled, options.Enabled ? 1 : 0);

        lock (_lock)
        {
            _minFovDegrees = options.MinFovDegrees;
            _maxFovDegrees = options.MaxFovDegrees;
            _fovDegrees = ClampFov(options.FovDegrees);
            _preferredBone = AimHelperBoneParser.Parse(options.PreferredBone);
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

    public void AdjustFovDegrees(float delta)
    {
        lock (_lock)
            _fovDegrees = ClampFov(_fovDegrees + delta);
    }

    private float ClampFov(float value) =>
        Math.Clamp(value, _minFovDegrees, _maxFovDegrees);
}
