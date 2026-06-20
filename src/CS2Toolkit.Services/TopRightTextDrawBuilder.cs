using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

internal static class TopRightTextDrawBuilder
{
    private const float CharWidthFactor = 0.55f;

    public static IReadOnlyList<DrawCommand> Build(
        int screenWidth,
        float margin,
        IReadOnlyList<StatusToast> toasts,
        uint defaultColor,
        float fontSize,
        int zIndex)
    {
        if (screenWidth <= 0 || toasts.Count == 0)
            return [];

        var commands = new List<DrawCommand>();
        var lineHeight = fontSize + 6f;
        var y = margin;

        foreach (var toast in toasts)
        {
            var color = toast.ColorArgb != 0 ? toast.ColorArgb : defaultColor;
            var width = toast.Message.Length * fontSize * CharWidthFactor;
            var x = Math.Max(margin, screenWidth - width - margin);

            commands.Add(new TextDrawCommand(x, y, toast.Message, color, fontSize, zIndex));
            y += lineHeight;
        }

        return commands;
    }
}
