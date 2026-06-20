using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Utilities;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Overlay;

public sealed class OverlayLayer
{
    private readonly object _lock = new();
    private Action<Graphics>? _drawAction;

    public string Name { get; }
    public int ZIndex { get; set; }

    public OverlayLayer(string name, int zIndex)
    {
        Name = name;
        ZIndex = zIndex;
    }

    public void QueueDraw(Action<Graphics> drawAction)
    {
        lock (_lock)
        {
            _drawAction = drawAction;
        }
    }

    public void ClearQueue()
    {
        lock (_lock)
        {
            _drawAction = null;
        }
    }

    public void Render(Graphics graphics)
    {
        Action<Graphics>? drawAction;
        lock (_lock)
        {
            drawAction = _drawAction;
        }

        drawAction?.Invoke(graphics);
    }
}

internal sealed class OverlayForm : Form
{
    private readonly ScreenOverlayManager _manager;

    public OverlayForm(ScreenOverlayManager manager)
    {
        _manager = manager;

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

    public void PresentFrame() => _manager.PresentFrame(this);

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

public sealed class ScreenOverlayManager : IDisposable
{
    private const byte AcSrcOver = 0;
    private const byte AcSrcAlpha = 1;
    private const int UlwAlpha = 2;

    private readonly object _layerLock = new();
    private readonly Dictionary<string, OverlayLayer> _layers = new(StringComparer.OrdinalIgnoreCase);
    private readonly System.Windows.Forms.Timer _renderTimer;
    private readonly Stopwatch _frameClock = Stopwatch.StartNew();
    private readonly long _minFrameTicks;
    private readonly Font _fpsFont = new("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Point);
    private readonly SolidBrush _fpsBrush = new(Color.FromArgb(220, Color.White));

    private OverlayLayer[] _sortedLayers = [];
    private bool _layersDirty = true;
    private int _topMostRefreshTicks;
    private long _lastPresentTicks;

    private Thread? _uiThread;
    private OverlayForm? _form;
    private bool _isReady;

    private Bitmap? _frameBuffer;
    private int _bufferWidth;
    private int _bufferHeight;

    private int _framesThisSecond;
    private int _currentFps;
    private string _fpsLabel = "0 FPS";
    private float _fpsLabelWidth;
    private DateTime _fpsWindowStart = DateTime.UtcNow;

    public bool IsReady => _isReady;

    public ScreenOverlayManager(IOptions<ToolkitOptions> options)
    {
        var targetFps = options.Value.Overlay.TargetFps;
        _minFrameTicks = targetFps > 0
            ? Stopwatch.Frequency / targetFps
            : 0;

        _renderTimer = new System.Windows.Forms.Timer { Interval = 1 };
        _renderTimer.Tick += OnRenderTick;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _uiThread = new Thread(() =>
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _form = new OverlayForm(this);
            _renderTimer.Start();
            _isReady = true;
            tcs.TrySetResult();

            Application.Run(_form);
        })
        {
            IsBackground = true,
            Name = "OverlayUIThread"
        };

        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();

        return tcs.Task.WaitAsync(cancellationToken);
    }

    public void EnsureOnTop()
    {
        if (_form is null || _form.IsDisposed)
            return;

        if (_form.InvokeRequired)
            _form.BeginInvoke(() => _form.EnsureOnTop());
        else
            _form.EnsureOnTop();
    }

    public OverlayLayer GetOrCreateLayer(string name, int zIndex)
    {
        lock (_layerLock)
        {
            if (_layers.TryGetValue(name, out var existing))
            {
                existing.ZIndex = zIndex;
                _layersDirty = true;
                return existing;
            }

            var layer = new OverlayLayer(name, zIndex);
            _layers[name] = layer;
            _layersDirty = true;
            return layer;
        }
    }

    public void Render(Graphics graphics)
    {
        foreach (var layer in GetSortedLayers())
            layer.Render(graphics);

        RecordFrame(graphics);
        DrawFpsLabel(graphics);
    }

    internal void PresentFrame(OverlayForm form)
    {
        var width = form.Width;
        var height = form.Height;
        if (width <= 0 || height <= 0)
            return;

        EnsureFrameBuffer(width, height);

        using (var graphics = Graphics.FromImage(_frameBuffer!))
        {
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
            Render(graphics);
        }

        BlitLayered(form, width, height);
    }

    public void SetInteractive(bool interactive)
    {
        if (_form is null || _form.IsDisposed)
            return;

        if (_form.InvokeRequired)
            _form.BeginInvoke(() => _form.SetClickThrough(!interactive));
        else
            _form.SetClickThrough(!interactive);
    }

    private OverlayLayer[] GetSortedLayers()
    {
        if (!_layersDirty)
            return _sortedLayers;

        lock (_layerLock)
        {
            if (!_layersDirty)
                return _sortedLayers;

            _sortedLayers = _layers.Values.OrderBy(l => l.ZIndex).ToArray();
            _layersDirty = false;
            return _sortedLayers;
        }
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

    private void DrawFpsLabel(Graphics graphics)
    {
        var x = graphics.VisibleClipBounds.Width - _fpsLabelWidth - 16f;
        graphics.DrawString(_fpsLabel, _fpsFont, _fpsBrush, x, 16f);
    }

    private void RecordFrame(Graphics graphics)
    {
        _framesThisSecond++;
        var elapsed = (DateTime.UtcNow - _fpsWindowStart).TotalSeconds;
        if (elapsed < 1d)
            return;

        _currentFps = (int)Math.Round(_framesThisSecond / elapsed);
        _framesThisSecond = 0;
        _fpsWindowStart = DateTime.UtcNow;
        _fpsLabel = $"{_currentFps} FPS";
        _fpsLabelWidth = graphics.MeasureString(_fpsLabel, _fpsFont).Width;
    }

    private void OnRenderTick(object? sender, EventArgs e)
    {
        if (_form is null || _form.IsDisposed)
            return;

        var now = _frameClock.ElapsedTicks;
        if (_minFrameTicks > 0 && now - _lastPresentTicks < _minFrameTicks)
            return;

        _lastPresentTicks = now;
        _form.PresentFrame();

        _topMostRefreshTicks++;
        if (_topMostRefreshTicks >= 120)
        {
            _topMostRefreshTicks = 0;
            _form.EnsureOnTop();
        }
    }

    public void Dispose()
    {
        _renderTimer.Stop();
        _renderTimer.Dispose();
        _frameBuffer?.Dispose();
        _fpsFont.Dispose();
        _fpsBrush.Dispose();

        if (_form is not null && !_form.IsDisposed)
        {
            if (_form.InvokeRequired)
                _form.BeginInvoke(_form.Close);
            else
                _form.Close();
        }
    }

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
