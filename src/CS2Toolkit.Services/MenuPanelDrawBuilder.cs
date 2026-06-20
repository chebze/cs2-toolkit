using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Services;

internal static class MenuPanelDrawBuilder
{
    private const float Padding = 12f;
    private const float CharWidthFactor = 0.55f;

    public static IReadOnlyList<DrawCommand> Build(
        float x,
        float y,
        IReadOnlyList<string> lines,
        uint backgroundColor,
        uint textColor,
        float fontSize,
        int zIndex)
    {
        if (lines.Count == 0)
            return [];

        var lineHeight = fontSize + 6f;
        var maxChars = lines.Max(static line => line.Length);
        var boxWidth = maxChars * fontSize * CharWidthFactor + Padding * 2f;
        var boxHeight = lines.Count * lineHeight + Padding * 2f;

        var commands = new List<DrawCommand>
        {
            new RectDrawCommand(
                x,
                y,
                boxWidth,
                boxHeight,
                backgroundColor,
                StrokeWidth: 0f,
                Filled: true,
                ZIndex: zIndex)
        };

        commands.AddRange(OverlayTextBuilder.BuildBlock(
            x + Padding,
            y + Padding,
            lines,
            textColor,
            fontSize,
            zIndex + 1));

        return commands;
    }
}
