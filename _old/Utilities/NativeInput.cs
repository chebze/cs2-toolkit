using System.Drawing;
using System.Runtime.InteropServices;

namespace Cs2Toolkit.Utilities;

public static class NativeInput
{
    private const int KeyStatePressed = 0x8000;

    public static bool IsKeyDown(Keys key) => (GetAsyncKeyState((int)key) & KeyStatePressed) != 0;

    public static (int X, int Y) GetCursorPosition()
    {
        if (!GetCursorPos(out var point))
            return (0, 0);

        return (point.X, point.Y);
    }

    public static MouseButtons GetPressedMouseButtons()
    {
        var buttons = MouseButtons.None;
        if (IsKeyDown(Keys.LButton)) buttons |= MouseButtons.Left;
        if (IsKeyDown(Keys.RButton)) buttons |= MouseButtons.Right;
        if (IsKeyDown(Keys.MButton)) buttons |= MouseButtons.Middle;
        if (IsKeyDown(Keys.XButton1)) buttons |= MouseButtons.XButton1;
        if (IsKeyDown(Keys.XButton2)) buttons |= MouseButtons.XButton2;
        return buttons;
    }

    public static void MoveMouseRelative(int deltaX, int deltaY)
    {
        if (deltaX == 0 && deltaY == 0)
            return;

        var input = new INPUT
        {
            type = InputMouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dx = deltaX,
                    dy = deltaY,
                    dwFlags = MouseeventfMove
                }
            }
        };

        _ = SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    public static void SetLeftButton(bool pressed)
    {
        var input = new INPUT
        {
            type = InputMouse,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    dwFlags = pressed ? MouseeventfLeftdown : MouseeventfLeftup
                }
            }
        };

        _ = SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    public static void SetKeyState(Keys key, bool pressed)
    {
        var input = new INPUT
        {
            type = InputKeyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)key,
                    dwFlags = pressed ? 0 : KeyeventfKeyup
                }
            }
        };

        _ = SendInput(1, [input], Marshal.SizeOf<INPUT>());
    }

    private const uint InputKeyboard = 1;
    private const uint KeyeventfKeyup = 0x0002;
    private const uint InputMouse = 0;
    private const uint MouseeventfMove = 0x0001;
    private const uint MouseeventfLeftdown = 0x0002;
    private const uint MouseeventfLeftup = 0x0004;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }
}
