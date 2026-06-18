using System.Drawing;
using System.Drawing.Drawing2D;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;

namespace Cs2Toolkit.Overlay;

internal static class GrenadeArcDrawer
{
    public static void DrawTrajectory(
        Graphics graphics,
        GrenadeTrajectorySnapshot snapshot,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        GrenadeOverlayOptions overlayOptions,
        float landingMarkerRadiusUnits)
    {
        if (!snapshot.IsActive || snapshot.Points.Count < 2)
            return;

        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var arcColor = DrawHelper.ParseColor(overlayOptions.ArcColor, Color.DeepSkyBlue);
        var landingColor = DrawHelper.ParseColor(overlayOptions.LandingColor, Color.Gold);

        using var arcPen = new Pen(arcColor, overlayOptions.ArcLineWidth)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        PointF? previous = null;
        foreach (var point in snapshot.Points)
        {
            if (!WorldToScreenHelper.TryProject(point, viewMatrix, screenWidth, screenHeight, out var screen))
            {
                previous = null;
                continue;
            }

            if (previous is { } last)
                graphics.DrawLine(arcPen, last, screen);

            previous = screen;
        }

        var landingPoint = snapshot.LandingPoint.IsValid
            ? snapshot.LandingPoint
            : snapshot.Points[^1];

        DrawLandingMarker(
            graphics,
            landingPoint,
            viewMatrix,
            screenWidth,
            screenHeight,
            landingColor,
            overlayOptions,
            landingMarkerRadiusUnits);
    }

    private static void DrawLandingMarker(
        Graphics graphics,
        Vector3 landingPoint,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        Color color,
        GrenadeOverlayOptions overlayOptions,
        float landingMarkerRadiusUnits)
    {
        if (WorldToScreenHelper.TryProject(landingPoint, viewMatrix, screenWidth, screenHeight, out var center))
        {
            var radius = Math.Max(3f, overlayOptions.LandingLineWidth * 2f);
            using var fill = new SolidBrush(color);
            graphics.FillEllipse(fill, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        }

        Span<PointF> polygon = stackalloc PointF[Math.Max(3, overlayOptions.LandingRingSegments)];
        if (!WorldToScreenHelper.TryProjectGroundRing(
                landingPoint,
                landingMarkerRadiusUnits,
                viewMatrix,
                screenWidth,
                screenHeight,
                polygon,
                out var pointCount)
            || pointCount < 3)
            return;

        using var pen = new Pen(color, overlayOptions.LandingLineWidth);
        graphics.DrawPolygon(pen, polygon[..pointCount].ToArray());
    }
}
