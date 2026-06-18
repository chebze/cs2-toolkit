using System.Drawing;
using System.Globalization;
using System.Text;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;

namespace Cs2Toolkit.Memory;

internal static class SkeletonDiagnostics
{
    private static readonly (int Id, string Name)[] LoggedBones =
    [
        (PlayerBones.Pelvis, "pelvis"),
        (PlayerBones.Neck, "neck"),
        (PlayerBones.Head, "head"),
        (PlayerBones.Chest, "chest"),
        (PlayerBones.ShoulderA, "shoulderA"),
        (PlayerBones.ElbowA, "elbowA"),
        (PlayerBones.HandA, "handA"),
        (PlayerBones.ShoulderB, "shoulderB"),
        (PlayerBones.ElbowB, "elbowB"),
        (PlayerBones.HandB, "handB"),
        (PlayerBones.HipA, "hipA"),
        (PlayerBones.KneeA, "kneeA"),
        (PlayerBones.AnkleA, "ankleA"),
        (PlayerBones.HipB, "hipB"),
        (PlayerBones.KneeB, "kneeB"),
        (PlayerBones.AnkleB, "ankleB")
    ];

    public static string FormatRead(
        EnemyLastSeenSnapshot snapshot,
        BoneReadContext context,
        bool hiddenFromLocalPlayer)
    {
        var builder = new StringBuilder();
        builder.Append(CultureInfo.InvariantCulture, $"[SkeletonRead] player={snapshot.Name} index={snapshot.PlayerIndex} hidden={hiddenFromLocalPlayer}");
        builder.Append(CultureInfo.InvariantCulture, $" pawn=0x{context.Pawn:X} scene=0x{context.SceneNode:X} boneArray=0x{context.BoneArray:X}");
        builder.Append(CultureInfo.InvariantCulture, $" origin={FormatVec(context.EntityOrigin)}");

        var pelvis = snapshot.Bones[PlayerBones.Pelvis];
        foreach (var (id, name) in LoggedBones)
        {
            var bone = snapshot.Bones[id];
            var dist = bone.IsValid && pelvis.IsValid ? bone.DistanceTo(pelvis) : -1f;
            builder.Append(CultureInfo.InvariantCulture,
                $"\n  {name}[{id}]={FormatVec(bone)} valid={bone.IsValid} dist_pelvis={dist:F1}");
        }

        builder.Append(CultureInfo.InvariantCulture, $"\n  unique_bone_positions={CountUniquePositions(snapshot.Bones)}");
        if (pelvis.IsValid && snapshot.Bones[PlayerBones.Head].IsValid)
        {
            builder.Append(CultureInfo.InvariantCulture,
                $" head_pelvis_3d={snapshot.Bones[PlayerBones.Head].DistanceTo(pelvis):F1}");
        }

        return builder.ToString();
    }

    public static string FormatDraw(
        EnemyLastSeenSnapshot snapshot,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight)
    {
        var builder = new StringBuilder();
        builder.Append(CultureInfo.InvariantCulture,
            $"[SkeletonDraw] player={snapshot.Name} index={snapshot.PlayerIndex} screen={screenWidth}x{screenHeight}");
        builder.Append(CultureInfo.InvariantCulture,
            $" view_w=({viewMatrix[12]:F3},{viewMatrix[13]:F3},{viewMatrix[14]:F3},{viewMatrix[15]:F3})");

        foreach (var (id, name) in LoggedBones)
        {
            var world = snapshot.Bones[id];
            if (!world.IsValid)
            {
                builder.Append(CultureInfo.InvariantCulture, $"\n  {name}[{id}] world=invalid screen=skip");
                continue;
            }

            if (WorldToScreenHelper.TryProject(world, viewMatrix, screenWidth, screenHeight, out var screen))
                builder.Append(CultureInfo.InvariantCulture, $"\n  {name}[{id}] world={FormatVec(world)} screen=({screen.X:F1},{screen.Y:F1})");
            else
                builder.Append(CultureInfo.InvariantCulture, $"\n  {name}[{id}] world={FormatVec(world)} screen=behind_camera");
        }

        foreach (var (from, to) in PlayerBones.Connections)
        {
            var start = snapshot.Bones[from];
            var end = snapshot.Bones[to];
            if (!start.IsValid || !end.IsValid)
                continue;

            var worldDist = start.DistanceTo(end);
            var startScreen = default(PointF);
            var endScreen = default(PointF);
            var hasScreen = WorldToScreenHelper.TryProject(start, viewMatrix, screenWidth, screenHeight, out startScreen)
                && WorldToScreenHelper.TryProject(end, viewMatrix, screenWidth, screenHeight, out endScreen);
            var screenDist = hasScreen
                ? MathF.Sqrt(MathF.Pow(startScreen.X - endScreen.X, 2f) + MathF.Pow(startScreen.Y - endScreen.Y, 2f))
                : -1f;

            builder.Append(CultureInfo.InvariantCulture,
                $"\n  link {from}->{to} world_dist={worldDist:F1} screen_dist={screenDist:F1}");
        }

        return builder.ToString();
    }

    private static int CountUniquePositions(ReadOnlySpan<Vector3> bones)
    {
        var positions = new List<Vector3>();
        foreach (var (id, _) in LoggedBones)
        {
            if (!bones[id].IsValid)
                continue;

            var bone = bones[id];
            if (positions.Any(existing => existing.DistanceTo(bone) < 1f))
                continue;

            positions.Add(bone);
        }

        return positions.Count;
    }

    private static string FormatVec(Vector3 value) =>
        $"({value.X:F1},{value.Y:F1},{value.Z:F1})";
}
