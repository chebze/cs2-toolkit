using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

internal static class SoundEspDrawBuilder
{
    private const int GroundRingSegments = 24;

    public static IReadOnlyList<DrawCommand> Build(
        SoundEspWaveState state,
        SoundEspProfileOptions options,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        var commands = new List<DrawCommand>();
        var baseColor = OverlayColorParser.ParseArgb(options.WaveColor, 0xFFE53935);
        var now = DateTimeOffset.UtcNow;

        foreach (var wave in state.Waves)
        {
            var progress = ComputeProgress(now, wave.StartedAt, options.WaveDurationMs);
            AddIndicator(commands, wave.WorldPosition, progress, baseColor, options, projector, viewMatrix, screenWidth, screenHeight, zIndex);
        }

        if (state.BombPosition is { IsValid: true } bombPosition)
        {
            var elapsedMs = (now - state.BombWaveEpoch).TotalMilliseconds;
            var progress = (float)(elapsedMs % options.WaveDurationMs / options.WaveDurationMs);
            AddIndicator(commands, bombPosition, progress, baseColor, options, projector, viewMatrix, screenWidth, screenHeight, zIndex);
        }

        return commands;
    }

    private static void AddIndicator(
        List<DrawCommand> commands,
        Vector3 worldPosition,
        float progress,
        uint baseColor,
        SoundEspProfileOptions options,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (options.Animation == SoundWaveAnimation.StaticBox)
        {
            TryAddStaticBox(commands, worldPosition, progress, baseColor, options, projector, viewMatrix, screenWidth, screenHeight, zIndex);
            return;
        }

        TryAddGroundRings(commands, worldPosition, progress, baseColor, options, projector, viewMatrix, screenWidth, screenHeight, zIndex);
    }

    private static void TryAddGroundRings(
        List<DrawCommand> commands,
        Vector3 worldCenter,
        float progress,
        uint baseColor,
        SoundEspProfileOptions options,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        Span<OverlayPoint> polygon = stackalloc OverlayPoint[GroundRingSegments];

        for (var ring = 0; ring < options.RingCount; ring++)
        {
            var ringProgress = progress - ring * options.RingSpacing;
            if (ringProgress is < 0f or > 1f)
                continue;

            var worldRadius = options.MinWorldRadius
                + ringProgress * (options.MaxWorldRadius - options.MinWorldRadius);
            var alpha = (byte)Math.Clamp((1f - ringProgress) * 220f, 0f, 255f);
            if (alpha == 0)
                continue;

            if (!TryProjectGroundRing(
                    worldCenter,
                    worldRadius,
                    projector,
                    viewMatrix,
                    screenWidth,
                    screenHeight,
                    polygon,
                    out var pointCount))
            {
                continue;
            }

            commands.Add(new PolylineDrawCommand(
                polygon[..pointCount].ToArray(),
                WithAlpha(baseColor, alpha),
                options.WaveLineWidth,
                Closed: true,
                ZIndex: zIndex));
        }
    }

    private static void TryAddStaticBox(
        List<DrawCommand> commands,
        Vector3 worldCenter,
        float progress,
        uint baseColor,
        SoundEspProfileOptions options,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        var worldRadius = options.MinWorldRadius
            + progress * (options.MaxWorldRadius - options.MinWorldRadius);
        var alpha = (byte)Math.Clamp((1f - progress) * 220f, 0f, 255f);
        if (alpha == 0)
            return;

        Span<OverlayPoint> polygon = stackalloc OverlayPoint[4];
        if (!TryProjectGroundRing(
                worldCenter,
                worldRadius,
                projector,
                viewMatrix,
                screenWidth,
                screenHeight,
                polygon,
                out _))
        {
            return;
        }

        if (!projector.TryProject(worldCenter, viewMatrix, screenWidth, screenHeight, out var centerX, out var centerY))
            return;

        var size = MathF.Max(8f, worldRadius * 0.15f);
        commands.Add(new RectDrawCommand(
            centerX - size,
            centerY - size,
            size * 2f,
            size * 2f,
            WithAlpha(baseColor, alpha),
            options.WaveLineWidth,
            Filled: false,
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

    private static float ComputeProgress(DateTimeOffset now, DateTimeOffset startedAt, int waveDurationMs)
    {
        if (waveDurationMs <= 0)
            return 1f;

        return (float)Math.Clamp((now - startedAt).TotalMilliseconds / waveDurationMs, 0d, 1d);
    }

    private static uint WithAlpha(uint argb, byte alpha) =>
        (argb & 0x00FFFFFF) | ((uint)alpha << 24);
}
