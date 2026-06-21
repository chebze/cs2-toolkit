using System.Runtime.InteropServices;
using CS2Toolkit.Drawing.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.WIC;

namespace CS2Toolkit.Drawing.Direct2D;

public sealed class Direct2DOverlayRenderer : IOverlayRenderer
{
    private const uint WmQuit = 0x0012;
    private const uint WmTimer = 0x0113;
    private readonly IOverlayFrameSource _frameSource;
    private readonly ILogger<Direct2DOverlayRenderer> _logger;

    private readonly ID2D1Factory1 _d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>(Vortice.Direct2D1.FactoryType.SingleThreaded);
    private readonly IWICImagingFactory _wicFactory = new IWICImagingFactory();
    private readonly IDWriteFactory _writeFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();

    private Direct2DOverlayHost? _host;
    private Thread? _uiThread;
    private OverlayWindow? _window;
    private nint _renderTimerId;
    private int _topMostRefreshTicks;
    private long _lastConsumedSequence = -1;

    public bool IsReady { get; private set; }

    public Direct2DOverlayRenderer(
        IOverlayFrameSource frameSource,
        ILogger<Direct2DOverlayRenderer> logger)
    {
        _frameSource = frameSource;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _uiThread = new Thread(() =>
        {
            _host = new Direct2DOverlayHost(_d2dFactory, _wicFactory, _writeFactory);
            _window = new OverlayWindow(_host);
            _renderTimerId = SetTimer(_window.Handle, 1, 1, nint.Zero);

            IsReady = true;
            _logger.LogInformation("Direct2D overlay renderer ready (uncapped present rate)");
            tcs.TrySetResult();

            RunMessageLoop();
        })
        {
            IsBackground = true,
            Name = "OverlayUIThread"
        };

        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();

        return tcs.Task.WaitAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_window is not null && _renderTimerId != nint.Zero)
            KillTimer(_window.Handle, _renderTimerId);

        if (_window is not null)
            PostMessage(_window.Handle, WmQuit, nint.Zero, nint.Zero);

        return Task.CompletedTask;
    }

    private void RunMessageLoop()
    {
        while (GetMessage(out var message, nint.Zero, 0, 0) > 0)
        {
            if (message.Message == WmTimer)
                OnRenderTick();

            TranslateMessage(ref message);
            DispatchMessage(ref message);
        }

        _window?.Dispose();
        _host?.Dispose();
        _writeFactory.Dispose();
        _wicFactory.Dispose();
        _d2dFactory.Dispose();
    }

    private void OnRenderTick()
    {
        if (_window is null)
            return;

        if (!_frameSource.TryGetLatest(out var frame) || frame.Sequence == _lastConsumedSequence)
            return;

        _lastConsumedSequence = frame.Sequence;
        _window.SetClickThrough(!frame.Interactive);
        _window.PresentFrame(frame);

        _topMostRefreshTicks++;
        if (_topMostRefreshTicks >= 120)
        {
            _topMostRefreshTicks = 0;
            _window.EnsureOnTop();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMessage
    {
        public nint Hwnd;
        public uint Message;
        public nint WParam;
        public nint LParam;
        public uint Time;
        public int PtX;
        public int PtY;
    }

    [DllImport("user32.dll")]
    private static extern int GetMessage(out NativeMessage lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref NativeMessage lpMsg);

    [DllImport("user32.dll")]
    private static extern nint DispatchMessage(ref NativeMessage lpMsg);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetTimer(nint hWnd, nint nIDEvent, uint uElapse, nint lpTimerFunc);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool KillTimer(nint hWnd, nint uIDEvent);
}
