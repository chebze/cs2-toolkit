using CS2Toolkit.Input.Abstractions;

namespace CS2Toolkit.Input;

public sealed class Win32InputSimulator : IInputSimulator
{
    public bool IsKeyDown(KeyCode key) => Win32InputNative.IsKeyDown(key.VirtualKey);

    public (int X, int Y) GetCursorPosition() => Win32InputNative.GetCursorPosition();

    public MouseButton GetPressedMouseButtons()
    {
        var buttons = MouseButton.None;
        if (IsKeyDown(new KeyCode((int)System.Windows.Forms.Keys.LButton))) buttons |= MouseButton.Left;
        if (IsKeyDown(new KeyCode((int)System.Windows.Forms.Keys.RButton))) buttons |= MouseButton.Right;
        if (IsKeyDown(new KeyCode((int)System.Windows.Forms.Keys.MButton))) buttons |= MouseButton.Middle;
        if (IsKeyDown(new KeyCode((int)System.Windows.Forms.Keys.XButton1))) buttons |= MouseButton.X1;
        if (IsKeyDown(new KeyCode((int)System.Windows.Forms.Keys.XButton2))) buttons |= MouseButton.X2;
        return buttons;
    }

    public void MoveMouseRelative(int deltaX, int deltaY) =>
        Win32InputNative.MoveMouseRelative(deltaX, deltaY);

    public void SetLeftButton(bool pressed) => Win32InputNative.SetLeftButton(pressed);

    public void SetKeyState(KeyCode key, bool pressed) =>
        Win32InputNative.SetKeyState(key.VirtualKey, pressed);
}
