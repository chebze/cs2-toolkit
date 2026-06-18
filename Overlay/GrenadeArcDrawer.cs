using System.Drawing;
using System.Drawing.Drawing2D;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Cs2Toolkit.Utilities;

namespace Cs2Toolkit.Overlay;

internal readonly struct GrenadeDrawStats
{
    public int ProjectedPoints { get; init; }
    public int DrawnSegments { get; init; }
    public bool LandingVisible { get; init; }
}

internal static class GrenadeArcDrawer
{
    public static GrenadeDrawStats DrawTrajectory(
        Graphics graphics,
        GrenadeTrajectorySnapshot snapshot,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        GrenadeOverlayOptions overlayOptions,
        float landingMarkerRadiusUnits)
    {
        if (!snapshot.IsActive || snapshot.Points.Count < 2)
            return default;

        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var arcColor = DrawHelper.ParseColor(overlayOptions.ArcColor, Color.DeepSkyBlue);
        var landingColor = DrawHelper.ParseColor(overlayOptions.LandingColor, Color.Gold);

        using var arcPen = new Pen(arcColor, overlayOptions.ArcLineWidth)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        var projectedPoints = 0;
        var drawnSegments = 0;
        var arcSegments = snapshot.Segments.Count > 0
            ? snapshot.Segments
            : [snapshot.Points];

        foreach (var arcSegment in arcSegments)
        {
            if (arcSegment.Count < 2)
                continue;

            var segmentStats = DrawArcSegment(graphics, arcPen, arcSegment, viewMatrix, screenWidth, screenHeight);
            projectedPoints += segmentStats.ProjectedPoints;
            drawnSegments += segmentStats.DrawnSegments;
        }

        var landingVisible = false;
        foreach (var bouncePoint in snapshot.BouncePoints)
        {
            if (DrawBounceMarker(graphics, bouncePoint, viewMatrix, screenWidth, screenHeight, landingColor, overlayOptions))
                landingVisible = true;
        }

        var landingPoint = snapshot.LandingPoint.IsValid
            ? snapshot.LandingPoint
            : snapshot.Points[^1];

        if (DrawLandingMarker(
                graphics,
                landingPoint,
                viewMatrix,
                screenWidth,
                screenHeight,
                landingColor,
                overlayOptions,
                landingMarkerRadiusUnits))
            landingVisible = true;

        return new GrenadeDrawStats
        {
            ProjectedPoints = projectedPoints,
            DrawnSegments = drawnSegments,
            LandingVisible = landingVisible
        };
    }

    private static GrenadeDrawStats DrawArcSegment(
        Graphics graphics,
        Pen arcPen,
        IReadOnlyList<Vector3> segment,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight)
    {
        PointF? previous = null;
        var projectedPoints = 0;
        var drawnSegments = 0;

        foreach (var point in segment)
        {
            if (!WorldToScreenHelper.TryProject(point, viewMatrix, screenWidth, screenHeight, out var screen))
            {
                previous = null;
                continue;
            }

            projectedPoints++;

            if (previous is { } last)
            {
                graphics.DrawLine(arcPen, last, screen);
                drawnSegments++;
            }

            previous = screen;
        }

        return new GrenadeDrawStats
        {
            ProjectedPoints = projectedPoints,
            DrawnSegments = drawnSegments
        };
    }

    private static bool DrawBounceMarker(
        Graphics graphics,
        Vector3 bouncePoint,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        Color color,
        GrenadeOverlayOptions overlayOptions)
    {
        if (!WorldToScreenHelper.TryProject(bouncePoint, viewMatrix, screenWidth, screenHeight, out var center))
            return false;

        var radius = Math.Max(2f, overlayOptions.LandingLineWidth * 1.5f);
        using var fill = new SolidBrush(color);
        graphics.FillEllipse(fill, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        return true;
    }

    private static bool DrawLandingMarker(
        Graphics graphics,
        Vector3 landingPoint,
        ReadOnlySpan<float> viewMatrix,
        int screenWidth,
        int screenHeight,
        Color color,
        GrenadeOverlayOptions overlayOptions,
        float landingMarkerRadiusUnits)
    {
        var landingVisible = false;

        if (WorldToScreenHelper.TryProject(landingPoint, viewMatrix, screenWidth, screenHeight, out var center))
        {
            landingVisible = true;
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
            return landingVisible;

        using var pen = new Pen(color, overlayOptions.LandingLineWidth);
        graphics.DrawPolygon(pen, polygon[..pointCount].ToArray());
        return true;
    }
}
