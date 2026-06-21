using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

internal static class BulletTracerDrawBuilder
{
    public static IReadOnlyList<DrawCommand> Build(
        BulletTracerState state,
        BulletTracerVisualOptions options,
        IWorldProjector projector,
        ViewMatrix viewMatrix,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (state.Tracers.Count == 0)
            return [];

        var commands = new List<DrawCommand>();
        var localColor = OverlayColorParser.ParseArgb(options.LocalColor, 0xFFFF4444);
        var teammateColor = OverlayColorParser.ParseArgb(options.TeammateColor, 0xFF44AAFF);
        var enemyColor = OverlayColorParser.ParseArgb(options.EnemyColor, 0xFFFF8800);
        var now = DateTimeOffset.UtcNow;

        foreach (var tracer in state.Tracers)
        {
            var progress = ComputeProgress(now, tracer.StartedAt, options.DurationMs);
            if (progress >= 1f)
                continue;

            if (!projector.TryProject(tracer.Start, viewMatrix, screenWidth, screenHeight, out var x1, out var y1)
                || !projector.TryProject(tracer.End, viewMatrix, screenWidth, screenHeight, out var x2, out var y2))
            {
                continue;
            }

            var baseColor = tracer.Kind switch
            {
                BulletTracerKind.Local => localColor,
                BulletTracerKind.Teammate => teammateColor,
                _ => enemyColor
            };
            var alpha = (byte)Math.Clamp((1f - progress) * 230f, 0f, 255f);
            if (alpha == 0)
                continue;

            commands.Add(new LineDrawCommand(
                x1,
                y1,
                x2,
                y2,
                WithAlpha(baseColor, alpha),
                options.LineWidth,
                ZIndex: zIndex));
        }

        return commands;
    }

    private static float ComputeProgress(DateTimeOffset now, DateTimeOffset startedAt, int durationMs)
    {
        if (durationMs <= 0)
            return 1f;

        return (float)Math.Clamp((now - startedAt).TotalMilliseconds / durationMs, 0d, 1d);
    }

    private static uint WithAlpha(uint argb, byte alpha) =>
        (argb & 0x00FFFFFF) | ((uint)alpha << 24);
}
