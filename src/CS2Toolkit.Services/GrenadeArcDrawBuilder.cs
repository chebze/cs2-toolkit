using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

internal static class GrenadeArcDrawBuilder
{
    private const int DefaultLandingRingSegments = 20;

    public static IReadOnlyList<DrawCommand> Build(
        GrenadeState grenade,
        GrenadeVisualOptions options,
        float landingMarkerRadiusUnits,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (!grenade.IsActive || grenade.Points.Count < 2)
            return [];

        var commands = new List<DrawCommand>();
        var arcColor = OverlayColorParser.ParseArgb(options.ArcColor, 0xFF38BDF8);
        var pointColor = OverlayColorParser.ParseArgb(options.PointColor, arcColor);
        var landingColor = OverlayColorParser.ParseArgb(options.LandingColor, 0xFFFBBF24);
        var impactColor = OverlayColorParser.ParseArgb(options.ImpactColor, landingColor);

        var arcSegments = grenade.Segments.Count > 0
            ? grenade.Segments
            : [grenade.Points];

        foreach (var segment in arcSegments)
            AddArcSegment(commands, segment, arcColor, pointColor, options.ArcLineWidth, projector, viewMatrix, screenWidth, screenHeight, zIndex);

        foreach (var bouncePoint in grenade.BouncePoints)
            TryAddFilledCircle(commands, bouncePoint, impactColor, MathF.Max(2f, options.LandingLineWidth * 1.5f), projector, viewMatrix, screenWidth, screenHeight, zIndex);

        var landingPoint = grenade.LandingPoint.IsValid
            ? grenade.LandingPoint
            : grenade.Points[^1];

        TryAddFilledCircle(commands, landingPoint, landingColor, MathF.Max(3f, options.LandingLineWidth * 2f), projector, viewMatrix, screenWidth, screenHeight, zIndex);
        TryAddLandingRing(
            commands,
            landingPoint,
            landingColor,
            landingMarkerRadiusUnits,
            options.LandingLineWidth,
            options.LandingRingSegments > 0 ? options.LandingRingSegments : DefaultLandingRingSegments,
            projector,
            viewMatrix,
            screenWidth,
            screenHeight,
            zIndex);

        return commands;
    }

    private static void AddArcSegment(
        List<DrawCommand> commands,
        IReadOnlyList<Vector3> segment,
        uint arcColor,
        uint pointColor,
        float lineWidth,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (segment.Count < 2)
            return;

        float? previousX = null;
        float? previousY = null;

        foreach (var point in segment)
        {
            if (!projector.TryProject(point, viewMatrix, screenWidth, screenHeight, out var x, out var y))
            {
                previousX = null;
                previousY = null;
                continue;
            }

            commands.Add(new CircleDrawCommand(x, y, 2f, pointColor, lineWidth, Filled: true, ZIndex: zIndex));

            if (previousX is not null && previousY is not null)
            {
                commands.Add(new LineDrawCommand(
                    previousX.Value,
                    previousY.Value,
                    x,
                    y,
                    arcColor,
                    lineWidth,
                    ZIndex: zIndex));
            }

            previousX = x;
            previousY = y;
        }
    }

    private static void TryAddFilledCircle(
        List<DrawCommand> commands,
        Vector3 worldPoint,
        uint color,
        float radius,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (!projector.TryProject(worldPoint, viewMatrix, screenWidth, screenHeight, out var x, out var y))
            return;

        commands.Add(new CircleDrawCommand(x, y, radius, color, radius, Filled: true, ZIndex: zIndex));
    }

    private static void TryAddLandingRing(
        List<DrawCommand> commands,
        Vector3 landingPoint,
        uint color,
        float landingMarkerRadiusUnits,
        float lineWidth,
        int segments,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        Span<OverlayPoint> polygon = stackalloc OverlayPoint[Math.Max(3, segments)];
        if (!TryProjectGroundRing(
                landingPoint,
                landingMarkerRadiusUnits,
                projector,
                viewMatrix,
                screenWidth,
                screenHeight,
                polygon,
                out var pointCount)
            || pointCount < 3)
        {
            return;
        }

        commands.Add(new PolylineDrawCommand(
            polygon[..pointCount].ToArray(),
            color,
            lineWidth,
            Closed: true,
            ZIndex: zIndex));
    }

    private static bool TryProjectGroundRing(
        Vector3 center,
        float worldRadius,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        Span<OverlayPoint> destination,
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

            if (!projector.TryProject(world, viewMatrix, screenWidth, screenHeight, out var x, out var y))
                continue;

            destination[pointCount++] = new OverlayPoint(x, y);
        }

        return pointCount >= 3;
    }
}
