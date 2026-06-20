using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Maps;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;
using NumericVector3 = System.Numerics.Vector3;

namespace CS2Toolkit.Game.Memory;

internal sealed class TriggerbotReader
{
    private const byte LifeStateDead = 2;

    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly MapVisibilityChecker _mapChecker;

    public TriggerbotReader(ProcessMemory memory, GameOffsets offsets, MapVisibilityChecker mapChecker)
    {
        _memory = memory;
        _offsets = offsets;
        _mapChecker = mapChecker;
    }

    public TriggerbotState Read(LegacyMemoryState state)
    {
        if (!_memory.IsAttached || !state.IsInMatch || state.LocalTeam == 0)
            return TriggerbotState.Inactive;

        var clientBase = _memory.ClientBase;
        var localPawn = _memory.ReadPtr(clientBase + _offsets.DwLocalPlayerPawn);
        var entityList = _memory.ReadPtr(clientBase + _offsets.DwEntityList);
        if (localPawn == nint.Zero || entityList == nint.Zero)
            return TriggerbotState.Inactive;

        if (!TryReadEyePosition(localPawn, out var eyePosition))
            return TriggerbotState.Inactive;

        var velocity = ReadVelocity(localPawn);
        var shotsFired = _offsets.M_iShotsFired != nint.Zero
            ? _memory.Read<int>(localPawn + _offsets.M_iShotsFired)
            : 0;
        var isReloading = IsReloading(entityList, localPawn);
        var crosshairOnEnemy = IsCrosshairOnEnemy(entityList, localPawn, state.LocalTeam);
        var localPlayerIndex = ResolveLocalPlayerIndex(state);
        var (nearestAngle, nearestId) = FindNearestVisibleEnemy(
            entityList,
            localPawn,
            state,
            localPlayerIndex,
            eyePosition);

        return new TriggerbotState(
            crosshairOnEnemy,
            isReloading,
            shotsFired,
            velocity,
            eyePosition,
            nearestAngle,
            nearestId);
    }

    private Vector3 ReadVelocity(nint localPawn)
    {
        if (_offsets.M_vecAbsVelocity == nint.Zero)
            return default;

        return new Vector3(
            _memory.Read<float>(localPawn + _offsets.M_vecAbsVelocity),
            _memory.Read<float>(localPawn + _offsets.M_vecAbsVelocity + 4),
            _memory.Read<float>(localPawn + _offsets.M_vecAbsVelocity + 8));
    }

    private bool IsReloading(nint entityList, nint localPawn)
    {
        if (_offsets.M_bInReload == nint.Zero || _offsets.M_pWeaponServices == nint.Zero)
            return false;

        var weaponServices = _memory.ReadPtr(localPawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var activeHandle = _memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return false;

        var weapon = ResolveEntityFromHandle(entityList, activeHandle);
        return weapon != nint.Zero
            && _memory.Read<byte>(weapon + _offsets.M_bInReload) != 0;
    }

    private bool IsCrosshairOnEnemy(nint entityList, nint localPawn, int localTeam)
    {
        if (_offsets.M_iIDEntIndex == nint.Zero)
            return false;

        var entIndex = _memory.Read<int>(localPawn + _offsets.M_iIDEntIndex);
        if (entIndex <= 0)
            return false;

        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var entity = ResolveEntityFromIndex(entityList, entIndex, spacing);
            if (entity != nint.Zero && IsEnemyEntity(entity, localTeam))
                return true;
        }

        return false;
    }

    private (float AngleDegrees, PlayerId? PlayerId) FindNearestVisibleEnemy(
        nint entityList,
        nint localPawn,
        LegacyMemoryState state,
        int localPlayerIndex,
        Vector3 eyePosition)
    {
        if (_offsets.M_angEyeAngles == nint.Zero)
            return (float.MaxValue, null);

        var viewPitch = _memory.Read<float>(localPawn + _offsets.M_angEyeAngles);
        var viewYaw = _memory.Read<float>(localPawn + _offsets.M_angEyeAngles + 4);
        var bestAngle = float.MaxValue;
        PlayerId? bestId = null;

        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == state.LocalTeam)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (localPlayerIndex >= 0 && !IsVisibleToLocalPlayer(pawn, localPlayerIndex))
                continue;

            var enemyPosition = BombSiteHelper.ReadEntityPosition(_memory, _offsets, pawn);
            if (!enemyPosition.IsValid)
                continue;

            var enemyVector = new Vector3(enemyPosition.X, enemyPosition.Y, enemyPosition.Z);
            if (_mapChecker.IsReady
                && !_mapChecker.TryHasLineOfSight(
                    ToNumeric(eyePosition),
                    ToNumeric(enemyVector)))
            {
                continue;
            }

            var angle = AngularDistanceDegrees(viewPitch, viewYaw, eyePosition, enemyVector);
            if (angle < bestAngle)
            {
                bestAngle = angle;
                bestId = new PlayerId(player.Index);
            }
        }

        return (bestAngle, bestId);
    }

    private static int ResolveLocalPlayerIndex(LegacyMemoryState state)
    {
        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer)
                return player.Index;
        }

        return -1;
    }

    private bool IsVisibleToLocalPlayer(nint pawn, int localPlayerIndex)
    {
        if (_offsets.M_entitySpottedState == nint.Zero)
            return false;

        var spottedState = pawn + _offsets.M_entitySpottedState;
        if (_memory.Read<byte>(spottedState + GameOffsets.EntitySpottedState_bSpotted) != 0)
            return true;

        if (localPlayerIndex < 0 || localPlayerIndex >= 64)
            return false;

        var maskIndex = localPlayerIndex / 32;
        var bit = localPlayerIndex % 32;
        var mask = _memory.Read<uint>(spottedState + GameOffsets.EntitySpottedState_bSpottedByMask + (nint)(maskIndex * 4));
        return (mask & (1u << bit)) != 0;
    }

    private bool TryReadEyePosition(nint localPawn, out Vector3 eyePosition)
    {
        var position = BombSiteHelper.ReadEntityPosition(_memory, _offsets, localPawn);
        if (!position.IsValid)
        {
            eyePosition = default;
            return false;
        }

        eyePosition = new Vector3(position.X, position.Y, position.Z);
        if (_offsets.M_vecViewOffset == nint.Zero)
            return true;

        var viewOffset = ReadVector(localPawn + _offsets.M_vecViewOffset);
        eyePosition = new Vector3(
            eyePosition.X + viewOffset.X,
            eyePosition.Y + viewOffset.Y,
            eyePosition.Z + viewOffset.Z);

        return true;
    }

    private static float AngularDistanceDegrees(
        float viewPitch,
        float viewYaw,
        Vector3 eyePosition,
        Vector3 targetPosition)
    {
        var dx = targetPosition.X - eyePosition.X;
        var dy = targetPosition.Y - eyePosition.Y;
        var dz = targetPosition.Z - eyePosition.Z;
        var distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        if (distance <= 0.01f)
            return 0f;

        var forwardX = MathF.Cos(DegreesToRadians(viewPitch)) * MathF.Cos(DegreesToRadians(viewYaw));
        var forwardY = MathF.Cos(DegreesToRadians(viewPitch)) * MathF.Sin(DegreesToRadians(viewYaw));
        var forwardZ = -MathF.Sin(DegreesToRadians(viewPitch));

        var dirX = dx / distance;
        var dirY = dy / distance;
        var dirZ = dz / distance;
        var dot = forwardX * dirX + forwardY * dirY + forwardZ * dirZ;
        dot = Math.Clamp(dot, -1f, 1f);

        return MathF.Acos(dot) * (180f / MathF.PI);
    }

    private static float DegreesToRadians(float degrees) => degrees * (MathF.PI / 180f);

    private bool IsEnemyEntity(nint entity, int localTeam)
    {
        if (_offsets.M_iTeamNum == nint.Zero || _offsets.M_iHealth == nint.Zero)
            return false;

        var team = _memory.Read<int>(entity + _offsets.M_iTeamNum);
        if (team == localTeam || team is not (GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist))
            return false;

        if (_offsets.M_lifeState != nint.Zero
            && _memory.Read<byte>(entity + _offsets.M_lifeState) == LifeStateDead)
            return false;

        return _memory.Read<int>(entity + _offsets.M_iHealth) > 0;
    }

    private nint ResolvePawnForPlayer(nint entityList, int index)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var controller = _memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
            if (controller == nint.Zero)
                continue;

            var pawnHandle = _memory.Read<uint>(controller + _offsets.M_hPlayerPawn);
            if (pawnHandle is 0 or 0xFFFFFFFF)
                return nint.Zero;

            return ResolveEntityFromHandle(entityList, pawnHandle);
        }

        return nint.Zero;
    }

    private nint ResolveEntityFromHandle(nint entityList, uint handle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var entity = ResolveEntityFromHandle(entityList, handle, spacing);
            if (entity != nint.Zero)
                return entity;
        }

        return nint.Zero;
    }

    private nint ResolveEntityFromHandle(nint entityList, uint handle, int spacing)
    {
        var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((handle & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return _memory.ReadPtr(listEntry + (nint)(spacing * (handle & 0x1FF)));
    }

    private nint ResolveEntityFromIndex(nint entityList, int index, int spacing)
    {
        var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return _memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
    }

    private Vector3 ReadVector(nint address) => new(
        _memory.Read<float>(address),
        _memory.Read<float>(address + 4),
        _memory.Read<float>(address + 8));

    private static NumericVector3 ToNumeric(Vector3 vector) => new(vector.X, vector.Y, vector.Z);
}
