using System.Diagnostics;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Services;

public sealed class OverlayComposer : IOverlayComposer
{
    private static readonly TimeSpan PresenterBudget = TimeSpan.FromMilliseconds(1);

    private readonly IReadOnlyList<IOverlayPresenter> _presenters;
    private readonly IWorldProjector _projector;
    private readonly IFeatureState _featureState;
    private readonly IStatusToastPublisher _statusToasts;
    private readonly ILogger<OverlayComposer> _logger;
    private long _sequence;

    public OverlayComposer(
        IEnumerable<IOverlayPresenter> presenters,
        IWorldProjector projector,
        IFeatureState featureState,
        IStatusToastPublisher statusToasts,
        ILogger<OverlayComposer> logger)
    {
        _presenters = presenters.ToList();
        _projector = projector;
        _featureState = featureState;
        _statusToasts = statusToasts;
        _logger = logger;
    }

    public OverlayFrame Compose(GameSnapshot snapshot, int screenWidth, int screenHeight)
    {
        var menuVisible = _featureState.IsEnabled(FeatureIds.Menu);
        var hasToasts = _statusToasts.HasActive;

        if (!snapshot.IsAttached)
        {
            if (!hasToasts)
                return new OverlayFrame(Interlocked.Increment(ref _sequence), DateTimeOffset.UtcNow, []);
        }

        var commands = new List<DrawCommand>();
        foreach (var presenter in _presenters)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                commands.AddRange(presenter.Present(snapshot, _projector, screenWidth, screenHeight));
                stopwatch.Stop();

                if (stopwatch.Elapsed > PresenterBudget)
                {
                    _logger.LogWarning(
                        "Overlay presenter {Layer} exceeded budget ({ElapsedMs:F2} ms)",
                        presenter.LayerName,
                        stopwatch.Elapsed.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Overlay presenter {Layer} failed", presenter.LayerName);
            }
        }

        commands.Sort(static (a, b) => a.ZIndex.CompareTo(b.ZIndex));
        return new OverlayFrame(
            Interlocked.Increment(ref _sequence),
            DateTimeOffset.UtcNow,
            commands,
            menuVisible);
    }
}
