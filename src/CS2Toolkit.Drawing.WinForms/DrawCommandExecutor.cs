using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Drawing.WinForms;

internal static class DrawCommandExecutor
{
    public static void Execute(Graphics graphics, IReadOnlyList<DrawCommand> commands)
    {
        foreach (var command in commands.OrderBy(static c => c.ZIndex))
            Execute(graphics, command);
    }

    private static void Execute(Graphics graphics, DrawCommand command)
    {
        switch (command)
        {
            case LineDrawCommand line:
                using (var pen = CreatePen(line.ColorArgb, line.StrokeWidth))
                    graphics.DrawLine(pen, line.X1, line.Y1, line.X2, line.Y2);
                break;

            case RectDrawCommand rect:
                using (var rectPen = CreatePen(rect.ColorArgb, rect.StrokeWidth))
                {
                    if (rect.Filled)
                    {
                        using var brush = CreateBrush(rect.ColorArgb);
                        graphics.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
                    }
                    else
                    {
                        graphics.DrawRectangle(rectPen, rect.X, rect.Y, rect.Width, rect.Height);
                    }
                }
                break;

            case CircleDrawCommand circle:
                using (var circlePen = CreatePen(circle.ColorArgb, circle.StrokeWidth))
                {
                    var diameter = circle.Radius * 2f;
                    var x = circle.CenterX - circle.Radius;
                    var y = circle.CenterY - circle.Radius;
                    if (circle.Filled)
                    {
                        using var brush = CreateBrush(circle.ColorArgb);
                        graphics.FillEllipse(brush, x, y, diameter, diameter);
                    }
                    else
                    {
                        graphics.DrawEllipse(circlePen, x, y, diameter, diameter);
                    }
                }
                break;

            case TextDrawCommand text:
                using (var brush = CreateBrush(text.ColorArgb))
                using (var font = new Font("Segoe UI", text.FontSize, FontStyle.Bold, GraphicsUnit.Point))
                    graphics.DrawString(text.Text, font, brush, text.X, text.Y);
                break;

            case PolylineDrawCommand polyline when polyline.Points.Count >= 2:
                using (var polyPen = CreatePen(polyline.ColorArgb, polyline.StrokeWidth))
                {
                    var points = polyline.Points.Select(static p => new PointF(p.X, p.Y)).ToArray();
                    if (polyline.Closed)
                        graphics.DrawPolygon(polyPen, points);
                    else
                        graphics.DrawLines(polyPen, points);
                }
                break;

            case ImageDrawCommand image when image.PngData.Length > 0:
            {
                using var stream = new MemoryStream(image.PngData);
                using var bitmap = new Bitmap(stream);
                graphics.DrawImage(bitmap, image.X, image.Y, image.Width, image.Height);
                break;
            }
        }
    }

    private static Pen CreatePen(uint colorArgb, float width)
    {
        var pen = new Pen(ToColor(colorArgb), width);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        return pen;
    }

    private static SolidBrush CreateBrush(uint colorArgb) => new(ToColor(colorArgb));

    private static Color ToColor(uint argb) =>
        Color.FromArgb(
            (int)((argb >> 24) & 0xFF),
            (int)((argb >> 16) & 0xFF),
            (int)((argb >> 8) & 0xFF),
            (int)(argb & 0xFF));
}
