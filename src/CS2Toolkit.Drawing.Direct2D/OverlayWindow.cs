using System.Runtime.InteropServices;

namespace CS2Toolkit.Drawing.Direct2D;

internal sealed class OverlayWindow : IDisposable
{
    private const string WindowClassName = "CS2ToolkitDirect2DOverlay";
    private const int WsExLayered = 0x80000;
    private const int WsExTransparent = 0x20;
    private const int WsExToolWindow = 0x80;
    private const int WsPopup = unchecked((int)0x80000000);
    private const int SwpNoActivate = 0x0010;
    private const int SwpShowWindow = 0x0040;
    private static readonly nint HwndTopmost = -1;

    private static readonly WndProcDelegate StaticWndProc = WndProc;
    private static bool _classRegistered;

    private readonly Direct2DOverlayHost _host;
    private readonly nint _hwnd;
    private OverlayBounds _bounds;

    public OverlayWindow(Direct2DOverlayHost host)
    {
        _host = host;
        RegisterClass();
        _bounds = GameWindowHelper.GetTargetBounds();

        _hwnd = CreateWindowEx(
            WsExLayered | WsExTransparent | WsExToolWindow,
            WindowClassName,
            string.Empty,
            WsPopup,
            _bounds.X,
            _bounds.Y,
            _bounds.Width,
            _bounds.Height,
            nint.Zero,
            nint.Zero,
            nint.Zero,
            nint.Zero);

        if (_hwnd == nint.Zero)
            throw new InvalidOperationException("Failed to create overlay window.");

        ShowWindow(_hwnd, 5); // SW_SHOW
        SetClickThrough(true);
        EnsureOnTop();
    }

    public nint Handle => _hwnd;
    public OverlayBounds Bounds => _bounds;

    public void SyncBounds()
    {
        _bounds = GameWindowHelper.GetTargetBounds();
        SetWindowPos(_hwnd, nint.Zero, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, SwpNoActivate);
    }

    public void EnsureOnTop()
    {
        SyncBounds();
        SetWindowPos(_hwnd, HwndTopmost, _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height, SwpNoActivate | SwpShowWindow);
    }

    public void SetClickThrough(bool enabled)
    {
        const int gwlExstyle = -20;

        var style = GetWindowLong(_hwnd, gwlExstyle);
        if (enabled)
            style |= WsExTransparent | WsExLayered | WsExToolWindow;
        else
            style &= ~WsExTransparent;

        SetWindowLong(_hwnd, gwlExstyle, style);
    }

    public void PresentFrame(CS2Toolkit.Drawing.Abstractions.OverlayFrame frame)
    {
        SyncBounds();
        _host.PresentFrame(_hwnd, _bounds, frame);
    }

    public void Dispose()
    {
        if (_hwnd != nint.Zero)
            DestroyWindow(_hwnd);
    }

    private static void RegisterClass()
    {
        if (_classRegistered)
            return;

        var wc = new WndClassEx
        {
            CbSize = (uint)Marshal.SizeOf<WndClassEx>(),
            Style = 0,
            LpfnWndProc = Marshal.GetFunctionPointerForDelegate(StaticWndProc),
            HInstance = GetModuleHandle(null),
            LpszClassName = WindowClassName
        };

        if (RegisterClassEx(ref wc) == 0)
            throw new InvalidOperationException("Failed to register overlay window class.");

        _classRegistered = true;
    }

    private static nint WndProc(nint hwnd, uint msg, nint wParam, nint lParam) =>
        DefWindowProc(hwnd, msg, wParam, lParam);

    private delegate nint WndProcDelegate(nint hwnd, uint msg, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WndClassEx
    {
        public uint CbSize;
        public uint Style;
        public nint LpfnWndProc;
        public int CbClsExtra;
        public int CbWndExtra;
        public nint HInstance;
        public nint HIcon;
        public nint HCursor;
        public nint HbrBackground;
        public string? LpszMenuName;
        public string LpszClassName;
        public nint HIconSm;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassEx(ref WndClassEx lpwcx);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        nint hWndParent,
        nint hMenu,
        nint hInstance,
        nint lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandle(string? lpModuleName);
}
