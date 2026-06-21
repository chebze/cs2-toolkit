using System.Runtime.InteropServices;
using System.Text;

namespace CS2Toolkit.Drawing.Direct2D;

internal static class GameWindowHelper
{
    private const int SmCxScreen = 0;
    private const int SmCyScreen = 1;

    private static readonly string[] Cs2WindowTitles =
    [
        "Counter-Strike 2",
        "Counter-Strike 2 Direct3D 9"
    ];

    public static OverlayBounds GetTargetBounds() =>
        TryGetCs2ClientBounds() ?? GetPrimaryScreenBounds();

    public static bool TryGetCs2WindowHandle(out nint handle)
    {
        foreach (var title in Cs2WindowTitles)
        {
            handle = FindWindow(nint.Zero, title);
            if (handle != nint.Zero)
                return true;
        }

        handle = FindCs2WindowByProcess();
        return handle != nint.Zero;
    }

    private static OverlayBounds? TryGetCs2ClientBounds()
    {
        if (!TryGetCs2WindowHandle(out var handle))
            return null;

        if (!GetClientRect(handle, out var clientRect))
            return null;

        var topLeft = new NativePoint { X = 0, Y = 0 };
        if (!ClientToScreen(handle, ref topLeft))
            return null;

        return new OverlayBounds(
            topLeft.X,
            topLeft.Y,
            clientRect.Right - clientRect.Left,
            clientRect.Bottom - clientRect.Top);
    }

    private static OverlayBounds GetPrimaryScreenBounds() =>
        new(0, 0, GetSystemMetrics(SmCxScreen), GetSystemMetrics(SmCyScreen));

    private static nint FindCs2WindowByProcess()
    {
        var processes = System.Diagnostics.Process.GetProcessesByName("cs2");
        if (processes.Length == 0)
            return nint.Zero;

        var processId = (uint)processes[0].Id;
        nint found = nint.Zero;

        EnumWindows((hwnd, _) =>
        {
            GetWindowThreadProcessId(hwnd, out var windowProcessId);
            if (windowProcessId != processId)
                return true;

            if (!IsWindowVisible(hwnd))
                return true;

            var length = GetWindowTextLength(hwnd);
            if (length == 0)
                return true;

            var builder = new StringBuilder(length + 1);
            GetWindowText(hwnd, builder, builder.Capacity);
            if (builder.Length == 0)
                return true;

            found = hwnd;
            return false;
        }, nint.Zero);

        return found;
    }

    private delegate bool EnumWindowsProc(nint hwnd, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindow(nint lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(nint hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(nint hWnd, out Rect lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(nint hWnd, ref NativePoint lpPoint);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
