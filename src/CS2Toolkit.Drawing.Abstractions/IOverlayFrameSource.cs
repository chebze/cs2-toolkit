namespace CS2Toolkit.Drawing.Abstractions;

public interface IOverlayFrameSource
{
    bool TryGetLatest(out OverlayFrame frame);
}
