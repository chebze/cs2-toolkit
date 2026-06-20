using System.Drawing;
using System.Drawing.Drawing2D;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;

namespace Cs2Toolkit.Overlay;

internal static class GroundWaveDrawer
{
    private const int GroundRingSegments = 24;

    public static void DrawRings(
        Graphics graphics,
        Vector3 worldCenter,
        float progress,
        Color color,
        EnemyNoiseOptions options,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        float lineWidth)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Span<PointF> polygon = stackalloc PointF[GroundRingSegments];

        for (var ring = 0; ring < options.RingCount; ring++)
        {
            var ringProgress = progress - ring * options.RingSpacing;
            if (ringProgress is < 0f or > 1f)
                continue;

            var worldRadius = options.MinWorldRadius
                + ringProgress * (options.MaxWorldRadius - options.MinWorldRadius);
            var alpha = (int)((1f - ringProgress) * 220f);
            if (alpha <= 0)
                continue;

            if (!WorldToScreenHelper.TryProjectGroundRing(
                    worldCenter,
                    worldRadius,
                    viewMatrix,
                    screenWidth,
                    screenHeight,
                    polygon,
                    out var pointCount))
                continue;

            var ringColor = Color.FromArgb(alpha, color);
            using var pen = new Pen(ringColor, lineWidth);
            graphics.DrawPolygon(pen, polygon[..pointCount].ToArray());
        }
    }
}
