namespace CS2Toolkit.Input.Abstractions;

public interface IInputListener
{
    event EventHandler<KeyInputEvent>? KeyDown;
    event EventHandler<KeyInputEvent>? KeyUp;
    event EventHandler<KeyInputEvent>? KeyPress;
    event EventHandler<MouseInputEvent>? MouseMove;
    event EventHandler<MouseInputEvent>? MouseDown;
}
