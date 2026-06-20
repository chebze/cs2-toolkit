using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Drawing.WinForms;

internal sealed class WinFormsOverlayViewport : IOverlayViewport
{
    public int Width => GameWindowHelper.GetTargetBounds().Width;
    public int Height => GameWindowHelper.GetTargetBounds().Height;
}
