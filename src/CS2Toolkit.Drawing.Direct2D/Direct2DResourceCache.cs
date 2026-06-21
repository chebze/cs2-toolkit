using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

namespace CS2Toolkit.Drawing.Direct2D;

internal sealed class Direct2DResourceCache : IDisposable
{
    private readonly ID2D1Factory _d2dFactory;
    private readonly IDWriteFactory _writeFactory;
    private readonly Dictionary<uint, ID2D1SolidColorBrush> _brushes = [];
    private readonly Dictionary<float, IDWriteTextFormat> _textFormats = [];
    private ID2D1StrokeStyle? _roundStrokeStyle;

    public Direct2DResourceCache(ID2D1Factory d2dFactory, IDWriteFactory writeFactory)
    {
        _d2dFactory = d2dFactory;
        _writeFactory = writeFactory;
    }

    public ID2D1SolidColorBrush GetBrush(ID2D1RenderTarget renderTarget, uint colorArgb)
    {
        if (_brushes.TryGetValue(colorArgb, out var cached))
            return cached;

        var brush = renderTarget.CreateSolidColorBrush(Direct2DColor.ToPremultiplied(colorArgb));
        _brushes[colorArgb] = brush;
        return brush;
    }

    public IDWriteTextFormat GetTextFormat(float fontSize)
    {
        if (_textFormats.TryGetValue(fontSize, out var cached))
            return cached;

        var format = _writeFactory.CreateTextFormat(
            "Segoe UI",
            null,
            FontWeight.Bold,
            FontStyle.Normal,
            FontStretch.Normal,
            fontSize,
            "en-us");

        _textFormats[fontSize] = format;
        return format;
    }

    public ID2D1StrokeStyle GetRoundStrokeStyle()
    {
        if (_roundStrokeStyle is not null)
            return _roundStrokeStyle;

        var props = new StrokeStyleProperties
        {
            StartCap = CapStyle.Round,
            EndCap = CapStyle.Round,
            LineJoin = LineJoin.Round
        };

        _roundStrokeStyle = _d2dFactory.CreateStrokeStyle(props);
        return _roundStrokeStyle;
    }

    public void InvalidateBrushes()
    {
        foreach (var brush in _brushes.Values)
            brush.Dispose();

        _brushes.Clear();
    }

    public void Dispose()
    {
        InvalidateBrushes();

        foreach (var format in _textFormats.Values)
            format.Dispose();

        _roundStrokeStyle?.Dispose();
        _textFormats.Clear();
    }
}

internal static class Direct2DColor
{
    public static Color4 ToPremultiplied(uint argb)
    {
        var alpha = ((argb >> 24) & 0xFF) / 255f;
        if (alpha <= 0f)
            return new Color4(0f, 0f, 0f, 0f);

        var red = ((argb >> 16) & 0xFF) / 255f * alpha;
        var green = ((argb >> 8) & 0xFF) / 255f * alpha;
        var blue = (argb & 0xFF) / 255f * alpha;
        return new Color4(red, green, blue, alpha);
    }
}
