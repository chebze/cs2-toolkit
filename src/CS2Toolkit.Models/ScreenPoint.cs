namespace CS2Toolkit.Models;

public readonly struct ScreenPoint(float x, float y, bool isVisible)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public bool IsVisible { get; } = isVisible;
}
