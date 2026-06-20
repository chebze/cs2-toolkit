using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Drawing.WinForms;

internal sealed class OverlayForm : Form
{
    private readonly WinFormsOverlayHost _host;

    public OverlayForm(WinFormsOverlayHost host)
    {
        _host = host;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Black;

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;

        SyncBounds();
    }

    public void SyncBounds()
    {
        var bounds = GameWindowHelper.GetTargetBounds();
        Location = bounds.Location;
        Size = bounds.Size;
    }

    public void EnsureOnTop()
    {
        SyncBounds();
        const int swpNoActivate = 0x0010;
        const int swpShowWindow = 0x0040;
        const nint hwndTopmost = -1;
        SetWindowPos(Handle, hwndTopmost, Left, Top, Width, Height, swpNoActivate | swpShowWindow);
    }

    public void PresentFrame(OverlayFrame frame) => _host.PresentFrame(this, frame);

    protected override CreateParams CreateParams
    {
        get
        {
            const int wsExLayered = 0x80000;
            const int wsExTransparent = 0x20;
            const int wsExToolWindow = 0x80;

            var cp = base.CreateParams;
            cp.ExStyle |= wsExLayered | wsExTransparent | wsExToolWindow;
            return cp;
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        SetClickThrough(true);
        EnsureOnTop();
    }

    public void SetClickThrough(bool enabled)
    {
        const int gwlExstyle = -20;
        const int wsExTransparent = 0x20;
        const int wsExLayered = 0x80000;
        const int wsExToolWindow = 0x80;

        var style = GetWindowLong(Handle, gwlExstyle);
        if (enabled)
            style |= wsExTransparent | wsExLayered | wsExToolWindow;
        else
            style &= ~wsExTransparent;

        SetWindowLong(Handle, gwlExstyle, style);
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(nint hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
}

internal sealed class WinFormsOverlayHost : IDisposable
{
    private const byte AcSrcOver = 0;
    private const byte AcSrcAlpha = 1;
    private const int UlwAlpha = 2;

    private Bitmap? _frameBuffer;
    private int _bufferWidth;
    private int _bufferHeight;
    private long _lastRenderedSequence = -1;

    public long LastRenderedSequence => Volatile.Read(ref _lastRenderedSequence);

    public void PresentFrame(OverlayForm form, OverlayFrame frame)
    {
        if (frame.Sequence == _lastRenderedSequence)
            return;

        var width = form.Width;
        var height = form.Height;
        if (width <= 0 || height <= 0)
            return;

        EnsureFrameBuffer(width, height);

        using (var graphics = Graphics.FromImage(_frameBuffer!))
        {
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            DrawCommandExecutor.Execute(graphics, frame.Commands);
        }

        BlitLayered(form, width, height);
        Volatile.Write(ref _lastRenderedSequence, frame.Sequence);
    }

    private void EnsureFrameBuffer(int width, int height)
    {
        if (_frameBuffer is not null && _bufferWidth == width && _bufferHeight == height)
            return;

        _frameBuffer?.Dispose();
        _frameBuffer = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        _bufferWidth = width;
        _bufferHeight = height;
    }

    private void BlitLayered(OverlayForm form, int width, int height)
    {
        var screenDc = GetDC(nint.Zero);
        var memDc = CreateCompatibleDC(screenDc);
        var hBitmap = _frameBuffer!.GetHbitmap(Color.FromArgb(0, 0, 0, 0));
        var oldBitmap = SelectObject(memDc, hBitmap);

        try
        {
            var topLeft = new Point(form.Left, form.Top);
            var size = new Size(width, height);
            var source = new Point(0, 0);
            var blend = new BlendFunction
            {
                BlendOp = AcSrcOver,
                SourceConstantAlpha = 255,
                AlphaFormat = AcSrcAlpha
            };

            UpdateLayeredWindow(form.Handle, screenDc, ref topLeft, ref size, memDc, ref source, 0, ref blend, UlwAlpha);
        }
        finally
        {
            SelectObject(memDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
            ReleaseDC(nint.Zero, screenDc);
        }
    }

    public void Dispose() => _frameBuffer?.Dispose();

    [StructLayout(LayoutKind.Sequential)]
    private struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint hWnd, nint hDC);

    [DllImport("gdi32.dll")]
    private static extern nint CreateCompatibleDC(nint hdc);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint hdc, nint hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(nint hdc);

    [DllImport("user32.dll")]
    private static extern bool UpdateLayeredWindow(
        nint hwnd,
        nint hdcDst,
        ref Point pptDst,
        ref Size psize,
        nint hdcSrc,
        ref Point pptSrc,
        int crKey,
        ref BlendFunction pblend,
        int dwFlags);
}
