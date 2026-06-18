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

        var projected = new List<PointF>(snapshot.Points.Count);
        foreach (var point in snapshot.Points)
        {
            if (WorldToScreenHelper.TryProject(point, viewMatrix, screenWidth, screenHeight, out var screen))
                projected.Add(screen);
        }

        for (var i = 1; i < projected.Count; i++)
            graphics.DrawLine(arcPen, projected[i - 1], projected[i]);

        if (!snapshot.LandingPoint.IsValid)
            return;

        DrawLandingMarker(
            graphics,
            snapshot.LandingPoint,
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

        if (WorldToScreenHelper.TryProject(landingPoint, viewMatrix, screenWidth, screenHeight, out var center))
        {
            var radius = Math.Max(3f, overlayOptions.LandingLineWidth * 2f);
            graphics.FillEllipse(new SolidBrush(color), center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        }
    }
}
