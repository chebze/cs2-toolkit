using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;
using System.Windows.Forms;

namespace Cs2Toolkit.Memory;

public sealed class RecoilCompensator
{
    private const float M_yaw = 0.022f;

    private GameOffsets? _offsets;
    private RcsOptions _options = new();
    private Vector3 _oldPunch;
    private int _lastShotsFired;
    private bool _compensateCurrentBullet = true;

    public void Initialize(GameOffsets offsets, RcsOptions options)
    {
        _offsets = offsets;
        _options = options;
        Reset();
    }

    public void Reset()
    {
        _oldPunch = default;
        _lastShotsFired = 0;
        _compensateCurrentBullet = true;
    }

    public void TryCompensate(ProcessMemory memory, nint clientBase, bool enabled) =>
        TryCompensateWithOptions(memory, clientBase, enabled, null);

    public void TryCompensateWithOptions(
        ProcessMemory memory,
        nint clientBase,
        bool enabled,
        RcsLayerSettings? weaponSettings)
    {
        if (_offsets is null || !enabled || !memory.IsAttached)
        {
            Reset();
            return;
        }

        if (!NativeInput.IsKeyDown(Keys.LButton))
        {
            Reset();
            return;
        }

        var localPawn = memory.ReadPtr(clientBase + _offsets.DwLocalPlayerPawn);
        if (localPawn == nint.Zero)
        {
            Reset();
            return;
        }

        if (_offsets.M_bIsScoped != nint.Zero
            && memory.Read<byte>(localPawn + _offsets.M_bIsScoped) != 0)
        {
            Reset();
            return;
        }

        var shotsFired = memory.Read<int>(localPawn + _offsets.M_iShotsFired);
        if (shotsFired <= 1)
        {
            Reset();
            return;
        }

        if (!TryReadAimPunch(memory, localPawn, out var currentPunch))
            return;

        var sensitivity = weaponSettings?.Sensitivity ?? _options.Sensitivity;
        var pitchScale = weaponSettings?.PitchScale ?? _options.PitchScale;
        var yawScale = weaponSettings?.YawScale ?? _options.YawScale;
        var firstBulletChance = weaponSettings?.FirstBulletCompensateChance ?? _options.FirstBulletCompensateChance;
        var skipChance = weaponSettings?.SubsequentBulletSkipChance ?? _options.SubsequentBulletSkipChance;

        UpdateBulletCompensationDecision(shotsFired, firstBulletChance, skipChance);

        var deltaPitch = (currentPunch.X - _oldPunch.X) * -1f;
        var deltaYaw = (currentPunch.Y - _oldPunch.Y) * -1f;

        if (_compensateCurrentBullet)
        {
            var mouseX = (int)(deltaYaw * yawScale / sensitivity / -M_yaw);
            var mouseY = (int)(deltaPitch * pitchScale / sensitivity / M_yaw);

            if (mouseX != 0 || mouseY != 0)
                NativeInput.MoveMouseRelative(mouseX, mouseY);
        }

        _oldPunch = currentPunch;
    }

    private void UpdateBulletCompensationDecision(
        int shotsFired,
        float firstBulletChance,
        float skipChance)
    {
        if (shotsFired == _lastShotsFired)
            return;

        if (shotsFired > _lastShotsFired)
        {
            _compensateCurrentBullet = shotsFired == 2
                ? Random.Shared.NextDouble() < firstBulletChance
                : Random.Shared.NextDouble() >= skipChance;
        }

        _lastShotsFired = shotsFired;
    }

    private bool TryReadAimPunch(ProcessMemory memory, nint localPawn, out Vector3 punch)
    {
        punch = default;

        if (_offsets!.M_pAimPunchServices == nint.Zero || _offsets.M_aimPunchCache == nint.Zero)
            return false;

        var aimPunchServices = memory.ReadPtr(localPawn + _offsets.M_pAimPunchServices);
        if (aimPunchServices == nint.Zero)
            return false;

        var cacheAddress = aimPunchServices + _offsets.M_aimPunchCache;
        var count = memory.Read<int>(cacheAddress);
        var data = memory.Read<nint>(cacheAddress + 8);

        if (count <= 0 || count > 0xFFFF || data == nint.Zero)
            return false;

        var angleAddress = data + (nint)((count - 1) * 12);
        punch = new Vector3(
            memory.Read<float>(angleAddress),
            memory.Read<float>(angleAddress + 4),
            memory.Read<float>(angleAddress + 8));

        return true;
    }
}
