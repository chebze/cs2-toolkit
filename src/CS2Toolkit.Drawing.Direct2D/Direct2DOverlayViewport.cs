using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Drawing.Direct2D;

internal sealed class Direct2DOverlayViewport : IOverlayViewport
{
    public int Width => GameWindowHelper.GetTargetBounds().Width;
    public int Height => GameWindowHelper.GetTargetBounds().Height;
}
