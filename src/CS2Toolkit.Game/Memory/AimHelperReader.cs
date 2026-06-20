using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Maps;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;
using NumericVector3 = System.Numerics.Vector3;

namespace CS2Toolkit.Game.Memory;

internal sealed class AimHelperReader
{
    private static readonly BoneId[] AimBones = [BoneId.Head, BoneId.Neck, BoneId.Chest];

    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly MapVisibilityChecker _mapChecker;

    public AimHelperReader(ProcessMemory memory, GameOffsets offsets, MapVisibilityChecker mapChecker)
    {
        _memory = memory;
        _offsets = offsets;
        _mapChecker = mapChecker;
    }

    public AimHelperState Read(LegacyMemoryState state, Vector3 eyePosition, Vector3 viewAngles)
    {
        if (!_memory.IsAttached || !state.IsInMatch || state.LocalTeam == 0 || !eyePosition.IsValid)
            return AimHelperState.Inactive;

        if (_offsets.M_angEyeAngles == nint.Zero)
            return AimHelperState.Inactive;

        var candidates = new List<AimCandidate>();

        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == state.LocalTeam)
                continue;

            if (!player.IsVisibleToLocalPlayer)
                continue;

            var bones = player.Bones;
            if (bones is null || !bones.HasValidSkeleton)
                continue;

            foreach (var bone in AimBones)
            {
                if (!bones.TryGetBone((int)bone, out var bonePosition))
                    continue;

                if (_mapChecker.IsReady
                    && !_mapChecker.TryHasLineOfSight(
                        ToNumeric(eyePosition),
                        ToNumeric(bonePosition)))
                {
                    continue;
                }

                var angle = AngularDistanceDegrees(
                    viewAngles.X,
                    viewAngles.Y,
                    eyePosition,
                    bonePosition);

                candidates.Add(new AimCandidate(
                    new PlayerId(player.Index),
                    bone,
                    bonePosition,
                    angle));
            }
        }

        return new AimHelperState(candidates);
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

    private static NumericVector3 ToNumeric(Vector3 vector) =>
        new(vector.X, vector.Y, vector.Z);
}
