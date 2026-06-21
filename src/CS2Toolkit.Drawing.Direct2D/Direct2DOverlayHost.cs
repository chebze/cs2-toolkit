using System.Runtime.InteropServices;
using CS2Toolkit.Drawing.Abstractions;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;

namespace CS2Toolkit.Drawing.Direct2D;

internal sealed class Direct2DOverlayHost : IDisposable
{
    private const byte AcSrcOver = 0;
    private const byte AcSrcAlpha = 1;
    private const int UlwAlpha = 2;

    private readonly ID2D1Factory1 _d2dFactory;
    private readonly IWICImagingFactory _wicFactory;
    private readonly Direct2DResourceCache _resourceCache;
    private readonly DrawCommandExecutor _executor;

    private IWICBitmap? _wicBitmap;
    private ID2D1RenderTarget? _renderTarget;
    private int _bufferWidth;
    private int _bufferHeight;
    private long _lastRenderedSequence = -1;

    public Direct2DOverlayHost(
        ID2D1Factory1 d2dFactory,
        IWICImagingFactory wicFactory,
        IDWriteFactory writeFactory)
    {
        _d2dFactory = d2dFactory;
        _wicFactory = wicFactory;
        _resourceCache = new Direct2DResourceCache(d2dFactory, writeFactory);
        _executor = new DrawCommandExecutor(d2dFactory, wicFactory, _resourceCache);
    }

    public long LastRenderedSequence => Volatile.Read(ref _lastRenderedSequence);

    public void PresentFrame(nint hwnd, OverlayBounds bounds, OverlayFrame frame)
    {
        if (frame.Sequence == _lastRenderedSequence)
            return;

        var width = bounds.Width;
        var height = bounds.Height;
        if (width <= 0 || height <= 0)
            return;

        EnsureRenderTarget(width, height);

        _renderTarget!.BeginDraw();
        _renderTarget.Clear(new Color4(0f, 0f, 0f, 0f));
        _executor.Execute(_renderTarget, frame.Commands);
        _renderTarget.EndDraw();

        BlitLayered(hwnd, bounds);
        Volatile.Write(ref _lastRenderedSequence, frame.Sequence);
    }

    private void EnsureRenderTarget(int width, int height)
    {
        if (_renderTarget is not null && _bufferWidth == width && _bufferHeight == height)
            return;

        _renderTarget?.Dispose();
        _wicBitmap?.Dispose();
        _resourceCache.InvalidateBrushes();

        _wicBitmap = _wicFactory.CreateBitmap(
            (uint)width,
            (uint)height,
            Vortice.WIC.PixelFormat.Format32bppPBGRA,
            BitmapCreateCacheOption.CacheOnLoad);

        var renderTargetProperties = new RenderTargetProperties(
            RenderTargetType.Default,
            new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied),
            96f,
            96f,
            RenderTargetUsage.GdiCompatible,
            FeatureLevel.Default);

        _renderTarget = _d2dFactory.CreateWicBitmapRenderTarget(_wicBitmap, renderTargetProperties);
        _renderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
        _bufferWidth = width;
        _bufferHeight = height;
    }

    private void BlitLayered(nint hwnd, OverlayBounds bounds)
    {
        var gdiInterop = _renderTarget!.QueryInterface<ID2D1GdiInteropRenderTarget>();
        var hdc = gdiInterop.GetDC(DcInitializeMode.Copy);

        try
        {
            var screenDc = GetDC(nint.Zero);
            try
            {
                var topLeft = new NativePoint(bounds.X, bounds.Y);
                var size = new NativeSize(bounds.Width, bounds.Height);
                var source = new NativePoint(0, 0);
                var blend = new BlendFunction
                {
                    BlendOp = AcSrcOver,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AcSrcAlpha
                };

                UpdateLayeredWindow(hwnd, screenDc, ref topLeft, ref size, hdc, ref source, 0, ref blend, UlwAlpha);
            }
            finally
            {
                ReleaseDC(nint.Zero, screenDc);
            }
        }
        finally
        {
            gdiInterop.ReleaseDC(null);
        }
    }

    public void Dispose()
    {
        _renderTarget?.Dispose();
        _wicBitmap?.Dispose();
        _resourceCache.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;

        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width;
        public int Height;

        public NativeSize(int width, int height)
        {
            Width = width;
            Height = height;
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

    [DllImport("user32.dll")]
    private static extern bool UpdateLayeredWindow(
        nint hwnd,
        nint hdcDst,
        ref NativePoint pptDst,
        ref NativeSize psize,
        nint hdcSrc,
        ref NativePoint pptSrc,
        int crKey,
        ref BlendFunction pblend,
        int dwFlags);
}
