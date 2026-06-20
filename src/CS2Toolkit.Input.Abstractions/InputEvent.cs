namespace CS2Toolkit.Input.Abstractions;

public enum InputEventKind
{
    KeyDown,
    KeyUp,
    KeyPress,
    MouseMove,
    MouseDown
}

public sealed class InputEvent
{
    public InputEventKind Kind { get; init; }
    public KeyCode Key { get; init; }
    public MouseButton Button { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class KeyInputEvent
{
    public KeyCode Key { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class MouseInputEvent
{
    public MouseButton Button { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
