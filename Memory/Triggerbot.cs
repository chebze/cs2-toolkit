using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;
using System.Windows.Forms;

namespace Cs2Toolkit.Memory;

public sealed class Triggerbot
{
    private const byte LifeStateDead = 2;

    private enum TriggerPhase
    {
        Idle,
        PreFire,
        OnTarget,
        PostFire
    }

    private GameOffsets? _offsets;
    private TbOptions _options = new();
    private MapVisibilityChecker? _mapVisibilityChecker;
    private readonly AutoStopper _autoStopper = new();
    private TriggerPhase _phase = TriggerPhase.Idle;
    private int _preFireBudget;
    private int _postFireBudget;
    private int _graceShotsBaseline;
    private bool _syntheticMouseDown;
    private int _reactionDelayMs;
    private DateTime _reactionAcquiredAt;

    public void Initialize(GameOffsets offsets, TbOptions options, MapVisibilityChecker? mapVisibilityChecker = null)
    {
        _offsets = offsets;
        _options = options;
        _mapVisibilityChecker = mapVisibilityChecker;
        Reset();
    }

    public void Reset()
    {
        ReleaseSyntheticFire();
        _autoStopper.Reset();
        _phase = TriggerPhase.Idle;
        _preFireBudget = 0;
        _postFireBudget = 0;
        _graceShotsBaseline = 0;
        _reactionDelayMs = 0;
    }

    public void TryTrigger(
        ProcessMemory memory,
        nint clientBase,
        MemoryState state,
        bool enabled,
        float preFireFovDegrees,
        int minReactionDelayMs,
        int maxReactionDelayMs,
        bool autoStopEnabled)
    {
        if (_offsets is null || !enabled || !memory.IsAttached || !state.IsInMatch || state.LocalTeam == 0)
        {
            Reset();
            return;
        }

        if (NativeInput.IsKeyDown(Keys.LButton))
        {
            Reset();
            return;
        }

        var localPawn = memory.ReadPtr(clientBase + _offsets.DwLocalPlayerPawn);
        var entityList = memory.ReadPtr(clientBase + _offsets.DwEntityList);
        if (localPawn == nint.Zero || entityList == nint.Zero)
        {
            Reset();
            return;
        }

        if (IsReloading(memory, entityList, localPawn))
        {
            ReleaseSyntheticFire();
            return;
        }

        var shotsFired = memory.Read<int>(localPawn + _offsets.M_iShotsFired);
        var localPlayerIndex = ResolveLocalPlayerIndex(memory, clientBase, entityList, state);
        var onTarget = IsCrosshairOnEnemy(memory, entityList, localPawn, state.LocalTeam);
        var nearTarget = !onTarget && IsNearVisibleEnemy(memory, entityList, localPawn, state, localPlayerIndex, preFireFovDegrees);
        var hasAcquisition = onTarget || nearTarget;

        if (!hasAcquisition && _phase != TriggerPhase.PostFire)
            ClearReactionDelay();
        else if (_phase == TriggerPhase.Idle && hasAcquisition)
            BeginReactionDelayIfNeeded(minReactionDelayMs, maxReactionDelayMs);

        var delayElapsed = IsReactionDelayElapsed();

        if (onTarget)
        {
            if (!delayElapsed)
            {
                ReleaseSyntheticFire();
                return;
            }

            if (_phase is TriggerPhase.Idle or TriggerPhase.PreFire)
                _postFireBudget = RollGraceBulletBudget();

            _phase = TriggerPhase.OnTarget;
            if (!TryBeginFiring(memory, localPawn, autoStopEnabled))
                return;

            return;
        }

        if (_phase == TriggerPhase.OnTarget)
        {
            _phase = TriggerPhase.PostFire;
            _graceShotsBaseline = shotsFired;
        }

        if (_phase == TriggerPhase.PostFire)
        {
            if (shotsFired < _graceShotsBaseline + _postFireBudget)
            {
                if (!TryBeginFiring(memory, localPawn, autoStopEnabled))
                    return;

                return;
            }

            ReleaseSyntheticFire();
            _phase = TriggerPhase.Idle;
            return;
        }

        if (nearTarget)
        {
            if (_phase == TriggerPhase.Idle)
            {
                _preFireBudget = RollGraceBulletBudget();
                _graceShotsBaseline = shotsFired;
                _phase = TriggerPhase.PreFire;
            }

            if (_phase == TriggerPhase.PreFire)
            {
                if (!delayElapsed)
                {
                    ReleaseSyntheticFire();
                    return;
                }

                if (shotsFired < _graceShotsBaseline + _preFireBudget)
                {
                    if (!TryBeginFiring(memory, localPawn, autoStopEnabled))
                        return;
                }
                else
                    ReleaseSyntheticFire();

                return;
            }
        }
        else if (_phase == TriggerPhase.PreFire)
        {
            ReleaseSyntheticFire();
            _phase = TriggerPhase.Idle;
            return;
        }

        ReleaseSyntheticFire();
        _phase = TriggerPhase.Idle;
    }

    private bool TryBeginFiring(ProcessMemory memory, nint localPawn, bool autoStopEnabled)
    {
        if (autoStopEnabled && !_autoStopper.TryEnsureStopped(memory, localPawn, _offsets!, _options))
        {
            ReleaseSyntheticFire();
            return false;
        }

        HoldSyntheticFire();
        return true;
    }

    private int RollGraceBulletBudget()
    {
        var min = Math.Min(_options.MinGraceBullets, _options.MaxGraceBullets);
        var max = Math.Max(_options.MinGraceBullets, _options.MaxGraceBullets);
        return Random.Shared.Next(min, max + 1);
    }

    private void BeginReactionDelayIfNeeded(int minReactionDelayMs, int maxReactionDelayMs)
    {
        if (_reactionDelayMs > 0)
            return;

        var min = Math.Min(minReactionDelayMs, maxReactionDelayMs);
        var max = Math.Max(minReactionDelayMs, maxReactionDelayMs);
        _reactionDelayMs = Random.Shared.Next(min, max + 1);
        _reactionAcquiredAt = DateTime.UtcNow;
    }

    private bool IsReactionDelayElapsed() =>
        _reactionDelayMs <= 0
        || (DateTime.UtcNow - _reactionAcquiredAt).TotalMilliseconds >= _reactionDelayMs;

    private void ClearReactionDelay() => _reactionDelayMs = 0;

    private void HoldSyntheticFire()
    {
        if (_syntheticMouseDown)
            return;

        NativeInput.SetLeftButton(true);
        _syntheticMouseDown = true;
    }

    private void ReleaseSyntheticFire()
    {
        if (!_syntheticMouseDown)
            return;

        NativeInput.SetLeftButton(false);
        _syntheticMouseDown = false;
    }

    private bool IsReloading(ProcessMemory memory, nint entityList, nint localPawn)
    {
        if (_offsets!.M_bInReload == nint.Zero || _offsets.M_pWeaponServices == nint.Zero)
            return false;

        var weaponServices = memory.ReadPtr(localPawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var activeHandle = memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return false;

        var weapon = ResolveEntityFromHandle(memory, entityList, activeHandle);
        if (weapon == nint.Zero)
            return false;

        return memory.Read<byte>(weapon + _offsets.M_bInReload) != 0;
    }

    private static System.Numerics.Vector3 ToVector3(Vector3 vector) =>
        new(vector.X, vector.Y, vector.Z);

    private bool IsCrosshairOnEnemy(
        ProcessMemory memory,
        nint entityList,
        nint localPawn,
        int localTeam)
    {
        if (_offsets!.M_iIDEntIndex == nint.Zero)
            return false;

        var entIndex = memory.Read<int>(localPawn + _offsets.M_iIDEntIndex);
        if (entIndex <= 0)
            return false;

        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var entity = ResolveEntityFromIndex(memory, entityList, entIndex, spacing);
            if (entity == nint.Zero)
                continue;

            if (IsEnemyEntity(memory, entity, localTeam))
                return true;
        }

        return false;
    }

    private bool IsNearVisibleEnemy(
        ProcessMemory memory,
        nint entityList,
        nint localPawn,
        MemoryState state,
        int localPlayerIndex,
        float preFireFovDegrees)
    {
        if (_offsets!.M_angEyeAngles == nint.Zero)
            return false;

        if (!TryReadEyePosition(memory, localPawn, out var eyePosition))
            return false;

        var viewPitch = memory.Read<float>(localPawn + _offsets.M_angEyeAngles);
        var viewYaw = memory.Read<float>(localPawn + _offsets.M_angEyeAngles + 4);

        var bestAngle = float.MaxValue;
        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == state.LocalTeam)
                continue;

            var pawn = ResolvePawnForPlayer(memory, entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (localPlayerIndex >= 0 && !IsVisibleToLocalPlayer(memory, pawn, localPlayerIndex))
                continue;

            var enemyPosition = BombSiteHelper.ReadEntityPosition(memory, _offsets, pawn);
            if (!enemyPosition.IsValid)
                continue;

            if (_mapVisibilityChecker is not null
                && !_mapVisibilityChecker.TryHasLineOfSight(
                    ToVector3(eyePosition),
                    ToVector3(enemyPosition)))
                continue;

            var angle = AngularDistanceDegrees(viewPitch, viewYaw, eyePosition, enemyPosition);
            if (angle < bestAngle)
                bestAngle = angle;
        }

        return bestAngle <= preFireFovDegrees;
    }

    private int ResolveLocalPlayerIndex(
        ProcessMemory memory,
        nint clientBase,
        nint entityList,
        MemoryState state)
    {
        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer)
                return player.Index;
        }

        var localController = memory.ReadPtr(clientBase + _offsets!.DwLocalPlayerController);
        if (localController == nint.Zero)
            return -1;

        for (var index = 1; index <= GameOffsets.MaxPlayerIndex; index++)
        {
            foreach (var spacing in GameOffsets.EntitySpacings)
            {
                var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
                if (listEntry == nint.Zero)
                    continue;

                var controller = memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
                if (controller == localController)
                    return index;
            }
        }

        return -1;
    }

    private bool IsVisibleToLocalPlayer(ProcessMemory memory, nint pawn, int localPlayerIndex)
    {
        if (pawn == nint.Zero || _offsets!.M_entitySpottedState == nint.Zero)
            return false;

        var spottedState = pawn + _offsets.M_entitySpottedState;

        if (memory.Read<byte>(spottedState + GameOffsets.EntitySpottedState_bSpotted) != 0)
            return true;

        if (localPlayerIndex >= 0 && localPlayerIndex < 64)
            return CheckSpottedMask(memory, spottedState, localPlayerIndex);

        return false;
    }

    private static bool CheckSpottedMask(ProcessMemory memory, nint spottedState, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex >= 64)
            return false;

        var maskIndex = bitIndex / 32;
        var bit = bitIndex % 32;
        var mask = memory.Read<uint>(spottedState + GameOffsets.EntitySpottedState_bSpottedByMask + (nint)(maskIndex * 4));
        return (mask & (1u << bit)) != 0;
    }

    private bool TryReadEyePosition(ProcessMemory memory, nint localPawn, out Vector3 eyePosition)
    {
        eyePosition = BombSiteHelper.ReadEntityPosition(memory, _offsets!, localPawn);
        if (!eyePosition.IsValid || _offsets!.M_vecViewOffset == nint.Zero)
            return eyePosition.IsValid;

        var viewOffset = ReadVector(memory, localPawn + _offsets.M_vecViewOffset);
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

    private bool IsEnemyEntity(ProcessMemory memory, nint entity, int localTeam)
    {
        if (_offsets!.M_iTeamNum == nint.Zero || _offsets.M_iHealth == nint.Zero)
            return false;

        var team = memory.Read<int>(entity + _offsets.M_iTeamNum);
        if (team == localTeam || team is not (GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist))
            return false;

        if (_offsets.M_lifeState != nint.Zero
            && memory.Read<byte>(entity + _offsets.M_lifeState) == LifeStateDead)
            return false;

        return memory.Read<int>(entity + _offsets.M_iHealth) > 0;
    }

    private nint ResolvePawnForPlayer(ProcessMemory memory, nint entityList, int index)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var controller = memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
            if (controller == nint.Zero)
                continue;

            var pawnHandle = memory.Read<uint>(controller + _offsets!.M_hPlayerPawn);
            if (pawnHandle is 0 or 0xFFFFFFFF)
                return nint.Zero;

            return ResolveEntityFromHandle(memory, entityList, pawnHandle);
        }

        return nint.Zero;
    }

    private nint ResolveEntityFromHandle(ProcessMemory memory, nint entityList, uint handle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var entity = ResolveEntityFromHandle(memory, entityList, handle, spacing);
            if (entity != nint.Zero)
                return entity;
        }

        return nint.Zero;
    }

    private static nint ResolveEntityFromHandle(ProcessMemory memory, nint entityList, uint handle, int spacing)
    {
        var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((handle & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return memory.ReadPtr(listEntry + (nint)(spacing * (handle & 0x1FF)));
    }

    private static nint ResolveEntityFromIndex(ProcessMemory memory, nint entityList, int index, int spacing)
    {
        var listEntry = memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
    }

    private static Vector3 ReadVector(ProcessMemory memory, nint address) =>
        new(
            memory.Read<float>(address),
            memory.Read<float>(address + 4),
            memory.Read<float>(address + 8));
}
