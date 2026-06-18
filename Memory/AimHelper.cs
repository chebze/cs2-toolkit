using Cs2Toolkit.Configuration;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;
using System.Windows.Forms;

namespace Cs2Toolkit.Memory;

public sealed class AimHelper
{
    private GameOffsets? _offsets;
    private MapVisibilityChecker? _mapVisibilityChecker;
    private ViewMatrixHolder? _viewMatrixHolder;
    private Keys _activationKey = Keys.None;
    private readonly Vector3[] _bones = new Vector3[PlayerBones.Count];
    private readonly float[] _viewMatrix = new float[16];

    public void Initialize(
        GameOffsets offsets,
        AimHelperOptions options,
        MapVisibilityChecker? mapVisibilityChecker,
        ViewMatrixHolder viewMatrixHolder)
    {
        _offsets = offsets;
        _mapVisibilityChecker = mapVisibilityChecker;
        _viewMatrixHolder = viewMatrixHolder;
        _activationKey = KeyParser.Parse(options.ActivationKey);
    }

    public void TryAim(
        ProcessMemory memory,
        nint clientBase,
        MemoryState state,
        bool enabled,
        float fovDegrees,
        AimHelperBone preferredBone)
    {
        if (_offsets is null || _viewMatrixHolder is null || !enabled || !memory.IsAttached
            || !state.IsInMatch || state.LocalTeam == 0)
            return;

        if (_activationKey != Keys.None && !NativeInput.IsKeyDown(_activationKey))
            return;

        var entityList = memory.ReadPtr(clientBase + _offsets.DwEntityList);
        var localPawn = memory.ReadPtr(clientBase + _offsets.DwLocalPlayerPawn);
        if (entityList == nint.Zero || localPawn == nint.Zero)
            return;

        if (!TryReadEyePosition(memory, localPawn, out var eyePosition))
            return;

        if (_offsets.M_angEyeAngles == nint.Zero)
            return;

        var viewPitch = memory.Read<float>(localPawn + _offsets.M_angEyeAngles);
        var viewYaw = memory.Read<float>(localPawn + _offsets.M_angEyeAngles + 4);
        var localPlayerIndex = ResolveLocalPlayerIndex(memory, clientBase, entityList, state);

        _viewMatrixHolder.CopyTo(_viewMatrix);
        var bounds = GameWindowHelper.GetTargetBounds();
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var bestAngle = float.MaxValue;
        Vector3 bestBonePosition = default;
        var foundTarget = false;

        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == state.LocalTeam)
                continue;

            var pawn = ResolvePawnForPlayer(memory, entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (localPlayerIndex >= 0 && !IsVisibleToLocalPlayer(memory, pawn, localPlayerIndex))
                continue;

            if (!BoneHelper.TryReadSkeleton(memory, _offsets, pawn, _bones))
                continue;

            if (!TryResolveBonePosition(preferredBone, _bones, out var bonePosition))
                continue;

            if (_mapVisibilityChecker is not null
                && !_mapVisibilityChecker.TryHasLineOfSight(
                    ToNumerics(eyePosition),
                    ToNumerics(bonePosition)))
                continue;

            var angle = AngularDistanceDegrees(viewPitch, viewYaw, eyePosition, bonePosition);
            if (angle > fovDegrees || angle >= bestAngle)
                continue;

            bestAngle = angle;
            bestBonePosition = bonePosition;
            foundTarget = true;
        }

        if (!foundTarget)
            return;

        if (!WorldToScreenHelper.TryProject(
                bestBonePosition,
                _viewMatrix,
                bounds.Width,
                bounds.Height,
                out var screen))
            return;

        var centerX = bounds.Width * 0.5f;
        var centerY = bounds.Height * 0.5f;
        var deltaX = (int)MathF.Round(screen.X - centerX);
        var deltaY = (int)MathF.Round(screen.Y - centerY);
        NativeInput.MoveMouseRelative(deltaX, deltaY);
    }

    private static bool TryResolveBonePosition(
        AimHelperBone preferred,
        ReadOnlySpan<Vector3> bones,
        out Vector3 position)
    {
        position = default;

        foreach (var boneId in GetBonePreferenceOrder(preferred))
        {
            if (boneId >= bones.Length || !bones[boneId].IsValid)
                continue;

            position = bones[boneId];
            return true;
        }

        return false;
    }

    private static IEnumerable<int> GetBonePreferenceOrder(AimHelperBone preferred) => preferred switch
    {
        AimHelperBone.Neck => [PlayerBones.Neck, PlayerBones.Head, PlayerBones.Chest],
        AimHelperBone.Body => [PlayerBones.Chest, PlayerBones.Neck, PlayerBones.Head],
        _ => [PlayerBones.Head, PlayerBones.Neck, PlayerBones.Chest]
    };

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

    private static System.Numerics.Vector3 ToNumerics(Vector3 vector) =>
        new(vector.X, vector.Y, vector.Z);

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

    private static Vector3 ReadVector(ProcessMemory memory, nint address) =>
        new(
            memory.Read<float>(address),
            memory.Read<float>(address + 4),
            memory.Read<float>(address + 8));
}
