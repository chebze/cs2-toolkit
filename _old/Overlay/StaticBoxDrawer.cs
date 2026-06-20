using System.Drawing;
using System.Drawing.Drawing2D;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;

namespace Cs2Toolkit.Overlay;

internal static class StaticBoxDrawer
{
    public static void DrawBox(
        Graphics graphics,
        Vector3 worldCenter,
        float progress,
        Color color,
        SoundEspProfileOptions options,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        float lineWidth)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var worldRadius = options.MinWorldRadius
            + progress * (options.MaxWorldRadius - options.MinWorldRadius);
        var alpha = (int)((1f - progress) * 220f);
        if (alpha <= 0)
            return;

        Span<PointF> polygon = stackalloc PointF[4];
        if (!WorldToScreenHelper.TryProjectGroundRing(
                worldCenter,
                worldRadius,
                viewMatrix,
                screenWidth,
                screenHeight,
                polygon,
                out _))
            return;

        if (!WorldToScreenHelper.TryProject(worldCenter, viewMatrix, screenWidth, screenHeight, out var center))
            return;

        var size = Math.Max(8f, worldRadius * 0.15f);
        var rect = new RectangleF(center.X - size, center.Y - size, size * 2f, size * 2f);
        var boxColor = Color.FromArgb(alpha, color);
        using var pen = new Pen(boxColor, lineWidth);
        graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }
}
