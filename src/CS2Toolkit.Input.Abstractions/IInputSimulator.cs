namespace CS2Toolkit.Input.Abstractions;

public interface IInputSimulator
{
    bool IsKeyDown(KeyCode key);
    (int X, int Y) GetCursorPosition();
    MouseButton GetPressedMouseButtons();
    void MoveMouseRelative(int deltaX, int deltaY);
    void SetLeftButton(bool pressed);
    void SetKeyState(KeyCode key, bool pressed);
}
