using System.Drawing;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Utilities;

public static class WorldToScreenHelper
{
    public static bool TryProject(
        Vector3 world,
        ReadOnlySpan<float> matrix,
        int screenWidth,
        int screenHeight,
        out PointF screen)
    {
        screen = default;

        if (matrix.Length < 16 || screenWidth <= 0 || screenHeight <= 0 || !world.IsValid)
            return false;

        var w = matrix[12] * world.X + matrix[13] * world.Y + matrix[14] * world.Z + matrix[15];
        if (w < 0.001f)
            return false;

        var clipX = matrix[0] * world.X + matrix[1] * world.Y + matrix[2] * world.Z + matrix[3];
        var clipY = matrix[4] * world.X + matrix[5] * world.Y + matrix[6] * world.Z + matrix[7];

        var invW = 1f / w;
        clipX *= invW;
        clipY *= invW;

        var x = screenWidth * 0.5f + clipX * screenWidth * 0.5f;
        var y = screenHeight * 0.5f - clipY * screenHeight * 0.5f;

        if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y))
            return false;

        screen = new PointF(x, y);
        return x >= -screenWidth && x <= screenWidth * 2f
            && y >= -screenHeight && y <= screenHeight * 2f;
    }

    public static bool TryProjectGroundRing(
        Vector3 center,
        float worldRadius,
        ReadOnlySpan<float> matrix,
        int screenWidth,
        int screenHeight,
        Span<PointF> destination,
        out int pointCount)
    {
        pointCount = 0;

        if (worldRadius <= 0f || destination.Length < 3 || !center.IsValid)
            return false;

        var segments = destination.Length;

        for (var i = 0; i < segments; i++)
        {
            var angle = MathF.Tau * i / segments;
            var world = new Vector3(
                center.X + worldRadius * MathF.Cos(angle),
                center.Y + worldRadius * MathF.Sin(angle),
                center.Z);

            if (!TryProject(world, matrix, screenWidth, screenHeight, out var screen))
                continue;

            destination[pointCount++] = screen;
        }

        return pointCount >= 3;
    }

    public static bool TryProjectGroundRing(
        Vector3 center,
        float worldRadius,
        ReadOnlySpan<float> matrix,
        int screenWidth,
        int screenHeight,
        int segments,
        out PointF[] polygon)
    {
        polygon = new PointF[segments];
        var valid = TryProjectGroundRing(
            center, worldRadius, matrix, screenWidth, screenHeight, polygon.AsSpan(), out var pointCount);

        if (!valid)
            return false;

        if (pointCount == segments)
            return true;

        polygon = polygon[..pointCount];
        return true;
    }
}
