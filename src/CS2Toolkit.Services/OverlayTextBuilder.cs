using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Services;

internal static class OverlayTextBuilder
{
    public static IReadOnlyList<DrawCommand> BuildBlock(
        float x,
        float y,
        IEnumerable<string> lines,
        uint colorArgb,
        float fontSize,
        int zIndex)
    {
        var commands = new List<DrawCommand>();
        var lineHeight = fontSize + 6f;
        var offsetY = y;

        foreach (var line in lines)
        {
            commands.Add(new TextDrawCommand(x, offsetY, line, colorArgb, fontSize, zIndex));
            offsetY += lineHeight;
        }

        return commands;
    }
}
