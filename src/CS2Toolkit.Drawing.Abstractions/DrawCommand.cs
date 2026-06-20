namespace CS2Toolkit.Drawing.Abstractions;

public abstract record DrawCommand(int ZIndex);

public sealed record LineDrawCommand(
    float X1,
    float Y1,
    float X2,
    float Y2,
    uint ColorArgb,
    float StrokeWidth,
    int ZIndex = 0) : DrawCommand(ZIndex);

public sealed record RectDrawCommand(
    float X,
    float Y,
    float Width,
    float Height,
    uint ColorArgb,
    float StrokeWidth,
    bool Filled = false,
    int ZIndex = 0) : DrawCommand(ZIndex);

public sealed record CircleDrawCommand(
    float CenterX,
    float CenterY,
    float Radius,
    uint ColorArgb,
    float StrokeWidth,
    bool Filled = false,
    int ZIndex = 0) : DrawCommand(ZIndex);

public sealed record TextDrawCommand(
    float X,
    float Y,
    string Text,
    uint ColorArgb,
    float FontSize,
    int ZIndex = 0) : DrawCommand(ZIndex);

public sealed record PolylineDrawCommand(
    IReadOnlyList<OverlayPoint> Points,
    uint ColorArgb,
    float StrokeWidth,
    bool Closed = false,
    int ZIndex = 0) : DrawCommand(ZIndex);

public sealed record ImageDrawCommand(
    float X,
    float Y,
    float Width,
    float Height,
    byte[] PngData,
    int ZIndex = 0) : DrawCommand(ZIndex);

public readonly struct OverlayPoint(float x, float y)
{
    public float X { get; } = x;
    public float Y { get; } = y;
}
