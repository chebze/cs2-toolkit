namespace CS2Toolkit.Input.Abstractions;

public interface IInputState
{
    bool IsKeyDown(KeyCode key);
    IReadOnlyCollection<KeyCode> HeldKeys { get; }
    MouseButton PressedMouseButtons { get; }
}
