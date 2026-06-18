using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Models;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Memory;

public sealed class GrenadeTrajectoryTracker
{
    private readonly GrenadeTrajectoryResolver _resolver;
    private readonly object _lock = new();
    private readonly float[] _viewMatrix = new float[16];

    private GameOffsets? _offsets;
    private MapVisibilityChecker? _mapChecker;
    private GrenadeTrajectorySnapshot _snapshot = new();

    public GrenadeTrajectoryTracker(IOptions<ToolkitOptions> options)
    {
        _resolver = new GrenadeTrajectoryResolver(options.Value.Grenade);
    }

    public ReadOnlySpan<float> LatestViewMatrix => _viewMatrix;

    public GrenadeTrajectorySnapshot Snapshot
    {
        get
        {
            lock (_lock)
                return _snapshot;
        }
    }

    public void Initialize(GameOffsets offsets, MapVisibilityChecker mapChecker)
    {
        _offsets = offsets;
        _mapChecker = mapChecker;
    }

    public void Poll(ProcessMemory memory, MemoryState state)
    {
        if (_offsets is null || _mapChecker is null || !memory.IsAttached || !state.IsInMatch)
        {
            Clear();
            return;
        }

        ReadViewMatrix(memory, _offsets);

        var snapshot = _resolver.Resolve(
            memory,
            _offsets,
            _mapChecker,
            memory.ClientBase,
            state);

        lock (_lock)
            _snapshot = snapshot;
    }

    private void ReadViewMatrix(ProcessMemory memory, GameOffsets offsets)
    {
        var matrixAddress = memory.ClientBase + offsets.DwViewMatrix;
        for (var i = 0; i < 16; i++)
            _viewMatrix[i] = memory.Read<float>(matrixAddress + (nint)(i * 4));
    }

    private void Clear()
    {
        lock (_lock)
            _snapshot = new GrenadeTrajectorySnapshot();
    }
}
