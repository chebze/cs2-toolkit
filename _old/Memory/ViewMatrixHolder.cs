using Cs2Toolkit.Models;

namespace Cs2Toolkit.Memory;

/// <summary>
/// Single source of truth for the game view-projection matrix used by all world-space overlays.
/// Updated every memory read while attached, independent of match/round state.
/// </summary>
public sealed class ViewMatrixHolder
{
    private readonly object _lock = new();
    private readonly float[] _matrix = new float[16];

    private GameOffsets? _offsets;

    public void Initialize(GameOffsets offsets) => _offsets = offsets;

    public void Update(ProcessMemory memory)
    {
        if (_offsets is null || !memory.IsAttached)
            return;

        var matrixAddress = memory.ClientBase + _offsets.DwViewMatrix;
        lock (_lock)
        {
            for (var i = 0; i < 16; i++)
                _matrix[i] = memory.Read<float>(matrixAddress + (nint)(i * 4));
        }
    }

    public void CopyTo(Span<float> destination)
    {
        if (destination.Length < 16)
            return;

        lock (_lock)
            _matrix.AsSpan().CopyTo(destination);
    }
}
