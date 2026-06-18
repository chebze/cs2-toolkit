using System.Drawing;
using System.Drawing.Drawing2D;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;

namespace Cs2Toolkit.Overlay;

internal static class SkeletonDrawer
{
    private const float HeadCircleScale = 0.45f;

    public static void DrawLastSeen(
        Graphics graphics,
        IEnumerable<EnemyLastSeenSnapshot> snapshots,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        Color color,
        float lineWidth)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var pen = new Pen(color, lineWidth);

        foreach (var snapshot in snapshots)
        {
            if (!snapshot.HasValidBones)
                continue;

            DrawHeadCircle(graphics, pen, snapshot, viewMatrix, screenWidth, screenHeight);
            DrawConnections(graphics, pen, snapshot, PlayerBones.Connections, viewMatrix, screenWidth, screenHeight);
        }
    }

    private static void DrawHeadCircle(
        Graphics graphics,
        Pen pen,
        EnemyLastSeenSnapshot snapshot,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight)
    {
        if (!TryGetBone(snapshot, PlayerBones.Head, out var headWorld)
            || !WorldToScreenHelper.TryProject(headWorld, viewMatrix, screenWidth, screenHeight, out var headScreen))
            return;

        var radius = 5f;
        if (TryGetBone(snapshot, PlayerBones.Neck, out var neckWorld)
            && WorldToScreenHelper.TryProject(neckWorld, viewMatrix, screenWidth, screenHeight, out var neckScreen))
        {
            var dx = headScreen.X - neckScreen.X;
            var dy = headScreen.Y - neckScreen.Y;
            radius = MathF.Max(3f, MathF.Sqrt(dx * dx + dy * dy) * HeadCircleScale);
        }

        graphics.DrawEllipse(
            pen,
            headScreen.X - radius,
            headScreen.Y - radius,
            radius * 2f,
            radius * 2f);
    }

    private static void DrawConnections(
        Graphics graphics,
        Pen pen,
        EnemyLastSeenSnapshot snapshot,
        ReadOnlySpan<(int From, int To)> connections,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight)
    {
        foreach (var (from, to) in connections)
        {
            if (!TryGetBone(snapshot, from, out var startWorld)
                || !TryGetBone(snapshot, to, out var endWorld))
                continue;

            if (startWorld.DistanceTo(endWorld) > PlayerBones.MaxConnectionWorldDistance)
                continue;

            if (!WorldToScreenHelper.TryProject(startWorld, viewMatrix, screenWidth, screenHeight, out var startScreen)
                || !WorldToScreenHelper.TryProject(endWorld, viewMatrix, screenWidth, screenHeight, out var endScreen))
                continue;

            graphics.DrawLine(pen, startScreen, endScreen);
        }
    }

    private static bool TryGetBone(EnemyLastSeenSnapshot snapshot, int boneIndex, out Vector3 bone)
    {
        bone = default;

        if (boneIndex < 0 || boneIndex >= snapshot.Bones.Length)
            return false;

        bone = snapshot.Bones[boneIndex];
        return bone.IsValid;
    }
}
