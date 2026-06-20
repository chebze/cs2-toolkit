using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Services;

internal readonly struct FovCircleLayout
{
    public float CenterX { get; init; }
    public float CenterY { get; init; }
    public float RadiusPixels { get; init; }
    public bool IsValid { get; init; }
}

internal static class FovCircleDrawBuilder
{
    public static IReadOnlyList<DrawCommand> BuildCenterCircle(
        float angularRadiusDegrees,
        float assumedHorizontalFovDegrees,
        uint color,
        float lineWidth,
        int screenWidth,
        int screenHeight,
        int zIndex)
    {
        if (!TryGetLayout(angularRadiusDegrees, assumedHorizontalFovDegrees, screenWidth, screenHeight, out var layout))
            return [];

        return
        [
            new CircleDrawCommand(
                layout.CenterX,
                layout.CenterY,
                layout.RadiusPixels,
                color,
                lineWidth,
                Filled: false,
                ZIndex: zIndex)
        ];
    }

    public static bool TryGetLayout(
        float angularRadiusDegrees,
        float assumedHorizontalFovDegrees,
        int screenWidth,
        int screenHeight,
        out FovCircleLayout layout)
    {
        layout = default;

        if (screenWidth <= 0 || screenHeight <= 0 || angularRadiusDegrees <= 0)
            return false;

        var halfHorizontalFovRad = assumedHorizontalFovDegrees * 0.5f * (MathF.PI / 180f);
        var aspect = screenHeight / (float)screenWidth;
        var halfVerticalFovRad = MathF.Atan(MathF.Tan(halfHorizontalFovRad) * aspect);
        var angularRadiusRad = angularRadiusDegrees * (MathF.PI / 180f);
        var radiusPixels = screenHeight * 0.5f * MathF.Tan(angularRadiusRad) / MathF.Tan(halfVerticalFovRad);
        if (radiusPixels <= 0.5f)
            return false;

        layout = new FovCircleLayout
        {
            CenterX = screenWidth * 0.5f,
            CenterY = screenHeight * 0.5f,
            RadiusPixels = radiusPixels,
            IsValid = true
        };

        return true;
    }
}
