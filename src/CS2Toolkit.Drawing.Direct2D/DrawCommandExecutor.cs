using System.Numerics;
using CS2Toolkit.Drawing.Abstractions;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using Vortice.WIC;

namespace CS2Toolkit.Drawing.Direct2D;

internal sealed class DrawCommandExecutor
{
    private readonly ID2D1Factory _d2dFactory;
    private readonly IWICImagingFactory _wicFactory;
    private readonly Direct2DResourceCache _resources;

    public DrawCommandExecutor(
        ID2D1Factory d2dFactory,
        IWICImagingFactory wicFactory,
        Direct2DResourceCache resources)
    {
        _d2dFactory = d2dFactory;
        _wicFactory = wicFactory;
        _resources = resources;
    }

    public void Execute(ID2D1RenderTarget renderTarget, IReadOnlyList<DrawCommand> commands)
    {
        foreach (var command in commands.OrderBy(static c => c.ZIndex))
            Execute(renderTarget, command);
    }

    private void Execute(ID2D1RenderTarget renderTarget, DrawCommand command)
    {
        switch (command)
        {
            case LineDrawCommand line:
                DrawLine(renderTarget, line);
                break;

            case RectDrawCommand rect:
                DrawRect(renderTarget, rect);
                break;

            case CircleDrawCommand circle:
                DrawCircle(renderTarget, circle);
                break;

            case TextDrawCommand text:
                DrawText(renderTarget, text);
                break;

            case PolylineDrawCommand polyline when polyline.Points.Count >= 2:
                DrawPolyline(renderTarget, polyline);
                break;

            case ImageDrawCommand image when image.PngData.Length > 0:
                DrawImage(renderTarget, image);
                break;
        }
    }

    private void DrawLine(ID2D1RenderTarget renderTarget, LineDrawCommand line)
    {
        var brush = _resources.GetBrush(renderTarget, line.ColorArgb);
        var stroke = _resources.GetRoundStrokeStyle();
        renderTarget.DrawLine(
            new Vector2(line.X1, line.Y1),
            new Vector2(line.X2, line.Y2),
            brush,
            line.StrokeWidth,
            stroke);
    }

    private void DrawRect(ID2D1RenderTarget renderTarget, RectDrawCommand rect)
    {
        var brush = _resources.GetBrush(renderTarget, rect.ColorArgb);
        var bounds = new Rect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);

        if (rect.Filled)
            renderTarget.FillRectangle(bounds, brush);
        else
            renderTarget.DrawRectangle(bounds, brush, rect.StrokeWidth, _resources.GetRoundStrokeStyle());
    }

    private void DrawCircle(ID2D1RenderTarget renderTarget, CircleDrawCommand circle)
    {
        var brush = _resources.GetBrush(renderTarget, circle.ColorArgb);
        var ellipse = new Ellipse(new Vector2(circle.CenterX, circle.CenterY), circle.Radius, circle.Radius);

        if (circle.Filled)
            renderTarget.FillEllipse(ellipse, brush);
        else
            renderTarget.DrawEllipse(ellipse, brush, circle.StrokeWidth, _resources.GetRoundStrokeStyle());
    }

    private void DrawText(ID2D1RenderTarget renderTarget, TextDrawCommand text)
    {
        var brush = _resources.GetBrush(renderTarget, text.ColorArgb);
        var format = _resources.GetTextFormat(text.FontSize);
        renderTarget.DrawText(text.Text, format, new Rect(text.X, text.Y, float.MaxValue, float.MaxValue), brush);
    }

    private void DrawPolyline(ID2D1RenderTarget renderTarget, PolylineDrawCommand polyline)
    {
        var brush = _resources.GetBrush(renderTarget, polyline.ColorArgb);
        var stroke = _resources.GetRoundStrokeStyle();
        var points = polyline.Points;

        if (polyline.Closed && points.Count >= 3)
        {
            using var geometry = CreatePathGeometry(points, closed: true);
            renderTarget.DrawGeometry(geometry, brush, polyline.StrokeWidth, stroke);
            return;
        }

        for (var i = 1; i < points.Count; i++)
        {
            var previous = points[i - 1];
            var current = points[i];
            renderTarget.DrawLine(
                new Vector2(previous.X, previous.Y),
                new Vector2(current.X, current.Y),
                brush,
                polyline.StrokeWidth,
                stroke);
        }
    }

    private ID2D1PathGeometry CreatePathGeometry(IReadOnlyList<OverlayPoint> points, bool closed)
    {
        var geometry = _d2dFactory.CreatePathGeometry();
        using var sink = geometry.Open();

        sink.BeginFigure(new Vector2(points[0].X, points[0].Y), FigureBegin.Hollow);
        for (var i = 1; i < points.Count; i++)
            sink.AddLine(new Vector2(points[i].X, points[i].Y));

        sink.EndFigure(closed ? FigureEnd.Closed : FigureEnd.Open);
        sink.Close();
        return geometry;
    }

    private void DrawImage(ID2D1RenderTarget renderTarget, ImageDrawCommand image)
    {
        using var stream = _wicFactory.CreateStream(image.PngData);
        using var decoder = _wicFactory.CreateDecoderFromStream(stream, DecodeOptions.CacheOnLoad);
        using var frame = decoder.GetFrame(0);
        using var converter = _wicFactory.CreateFormatConverter();
        converter.Initialize(frame, Vortice.WIC.PixelFormat.Format32bppPBGRA);

        using var bitmap = renderTarget.CreateBitmapFromWicBitmap(converter);
        var destination = new Rect(image.X, image.Y, image.X + image.Width, image.Y + image.Height);
        renderTarget.DrawBitmap(bitmap, 1.0f, Vortice.Direct2D1.BitmapInterpolationMode.Linear, destination);
    }
}
