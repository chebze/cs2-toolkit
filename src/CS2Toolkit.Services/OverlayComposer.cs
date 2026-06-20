using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class OverlayComposer : IOverlayComposer
{
    private readonly IReadOnlyList<IOverlayPresenter> _presenters;
    private readonly IWorldProjector _projector;
    private long _sequence;

    public OverlayComposer(IEnumerable<IOverlayPresenter> presenters, IWorldProjector projector)
    {
        _presenters = presenters.ToList();
        _projector = projector;
    }

    public OverlayFrame Compose(GameSnapshot snapshot, int screenWidth, int screenHeight)
    {
        if (!snapshot.IsAttached || !snapshot.IsInMatch)
            return new OverlayFrame(Interlocked.Increment(ref _sequence), DateTimeOffset.UtcNow, []);

        var commands = new List<DrawCommand>();
        foreach (var presenter in _presenters)
            commands.AddRange(presenter.Present(snapshot, _projector, screenWidth, screenHeight));

        commands.Sort(static (a, b) => a.ZIndex.CompareTo(b.ZIndex));
        return new OverlayFrame(Interlocked.Increment(ref _sequence), DateTimeOffset.UtcNow, commands);
    }
}
