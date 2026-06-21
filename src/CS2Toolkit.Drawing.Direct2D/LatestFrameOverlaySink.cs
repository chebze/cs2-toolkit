using CS2Toolkit.Drawing.Abstractions;

namespace CS2Toolkit.Drawing.Direct2D;

internal sealed class LatestFrameOverlaySink : IOverlayFrameSink, IOverlayFrameSource
{
    private OverlayFrame? _latest;

    public void Publish(OverlayFrame frame) =>
        Interlocked.Exchange(ref _latest, frame);

    public bool TryGetLatest(out OverlayFrame frame)
    {
        frame = _latest ?? OverlayFrame.Empty;
        return _latest is not null;
    }
}
