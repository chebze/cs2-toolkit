using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Memory;

internal sealed class RcsReader
{
    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;

    public RcsReader(ProcessMemory memory, GameOffsets offsets)
    {
        _memory = memory;
        _offsets = offsets;
    }

    public RcsState Read(bool isInMatch)
    {
        if (!_memory.IsAttached || !isInMatch)
            return RcsState.Inactive;

        var localPawn = _memory.ReadPtr(_memory.ClientBase + _offsets.DwLocalPlayerPawn);
        if (localPawn == nint.Zero)
            return RcsState.Inactive;

        var shotsFired = _offsets.M_iShotsFired != nint.Zero
            ? _memory.Read<int>(localPawn + _offsets.M_iShotsFired)
            : 0;
        var isScoped = _offsets.M_bIsScoped != nint.Zero
            && _memory.Read<byte>(localPawn + _offsets.M_bIsScoped) != 0;
        var hasAimPunch = TryReadAimPunch(localPawn, out var aimPunch);

        return new RcsState(aimPunch, shotsFired, isScoped, hasAimPunch);
    }

    private bool TryReadAimPunch(nint localPawn, out Vector3 punch)
    {
        punch = default;

        if (_offsets.M_pAimPunchServices == nint.Zero || _offsets.M_aimPunchCache == nint.Zero)
            return false;

        var aimPunchServices = _memory.ReadPtr(localPawn + _offsets.M_pAimPunchServices);
        if (aimPunchServices == nint.Zero)
            return false;

        var cacheAddress = aimPunchServices + _offsets.M_aimPunchCache;
        var count = _memory.Read<int>(cacheAddress);
        var data = _memory.Read<nint>(cacheAddress + 8);

        if (count <= 0 || count > 0xFFFF || data == nint.Zero)
            return false;

        var angleAddress = data + (nint)((count - 1) * 12);
        punch = new Vector3(
            _memory.Read<float>(angleAddress),
            _memory.Read<float>(angleAddress + 4),
            _memory.Read<float>(angleAddress + 8));

        return true;
    }
}
