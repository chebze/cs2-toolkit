using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cs2Toolkit.Utilities;

public readonly struct FovCircleLayout
{
    public float CenterX { get; init; }
    public float CenterY { get; init; }
    public float RadiusPixels { get; init; }
    public bool IsValid { get; init; }
}

public static class DrawHelper
{
    public static Color ParseColor(string hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallback;

        try
        {
            return ColorTranslator.FromHtml(hex);
        }
        catch
        {
            return fallback;
        }
    }

    public static void DrawTextBlock(Graphics graphics, int x, int y, IEnumerable<string> lines, Color color, int fontSize)
    {
        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Point);
        using var brush = new SolidBrush(color);

        var lineHeight = fontSize + 6;
        var offsetY = y;

        foreach (var line in lines)
        {
            graphics.DrawString(line, font, brush, x, offsetY);
            offsetY += lineHeight;
        }
    }

    public static void DrawTextTopRight(Graphics graphics, string text, Color color, int fontSize, int margin = 16)
    {
        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Point);
        using var brush = new SolidBrush(color);
        var size = graphics.MeasureString(text, font);
        var x = graphics.VisibleClipBounds.Width - size.Width - margin;
        graphics.DrawString(text, font, brush, x, margin);
    }

    public static void DrawTextBottomLeft(
        Graphics graphics,
        string text,
        Color color,
        int fontSize,
        int margin = 16,
        int lineIndexFromBottom = 0)
    {
        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Point);
        using var brush = new SolidBrush(color);
        var lineHeight = fontSize + 6;
        var bounds = graphics.VisibleClipBounds;
        var y = bounds.Height - margin - (lineIndexFromBottom + 1) * lineHeight;
        graphics.DrawString(text, font, brush, margin, y);
    }

    public static void DrawAngularFovCircle(
        Graphics graphics,
        float angularRadiusDegrees,
        Color color,
        float lineWidth,
        float assumedHorizontalFovDegrees = 90f)
    {
        var layout = GetAngularFovCircleLayout(graphics, angularRadiusDegrees, assumedHorizontalFovDegrees);
        if (!layout.IsValid)
            return;

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(color, lineWidth);
        graphics.DrawEllipse(
            pen,
            layout.CenterX - layout.RadiusPixels,
            layout.CenterY - layout.RadiusPixels,
            layout.RadiusPixels * 2f,
            layout.RadiusPixels * 2f);
    }

    public static FovCircleLayout GetAngularFovCircleLayout(
        Graphics graphics,
        float angularRadiusDegrees,
        float assumedHorizontalFovDegrees = 90f)
    {
        var bounds = graphics.VisibleClipBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0 || angularRadiusDegrees <= 0)
            return default;

        var halfHorizontalFovRad = assumedHorizontalFovDegrees * 0.5f * (MathF.PI / 180f);
        var aspect = bounds.Height / bounds.Width;
        var halfVerticalFovRad = MathF.Atan(MathF.Tan(halfHorizontalFovRad) * aspect);
        var angularRadiusRad = angularRadiusDegrees * (MathF.PI / 180f);
        var radiusPixels = bounds.Height * 0.5f * MathF.Tan(angularRadiusRad) / MathF.Tan(halfVerticalFovRad);
        if (radiusPixels <= 0.5f)
            return default;

        return new FovCircleLayout
        {
            CenterX = bounds.Width * 0.5f,
            CenterY = bounds.Height * 0.5f,
            RadiusPixels = radiusPixels,
            IsValid = true
        };
    }

    public static void DrawTextRightOfPoint(
        Graphics graphics,
        float anchorX,
        float anchorY,
        string[] lines,
        Color color,
        int fontSize,
        float gap = 8f)
    {
        if (lines.Length == 0)
            return;

        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Point);
        using var brush = new SolidBrush(color);

        var lineHeight = fontSize + 4;
        var totalHeight = lines.Length * lineHeight;
        var startY = anchorY - totalHeight * 0.5f;

        for (var i = 0; i < lines.Length; i++)
            graphics.DrawString(lines[i], font, brush, anchorX + gap, startY + i * lineHeight);
    }

    public static void DrawTextLeftOfPoint(
        Graphics graphics,
        float anchorX,
        float anchorY,
        string[] lines,
        Color color,
        int fontSize,
        float gap = 8f)
    {
        if (lines.Length == 0)
            return;

        using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Point);
        using var brush = new SolidBrush(color);

        var lineHeight = fontSize + 4;
        var totalHeight = lines.Length * lineHeight;
        var startY = anchorY - totalHeight * 0.5f;
        var maxWidth = 0f;
        foreach (var line in lines)
            maxWidth = Math.Max(maxWidth, graphics.MeasureString(line, font).Width);

        var x = anchorX - gap - maxWidth;
        for (var i = 0; i < lines.Length; i++)
            graphics.DrawString(lines[i], font, brush, x, startY + i * lineHeight);
    }
}
