using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class BulletTracerOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 105;

    private readonly IActiveConfiguration _configuration;
    private readonly IFeatureState _state;
    private readonly BulletTracerTracker _tracker;

    public BulletTracerOverlayPresenter(
        IActiveConfiguration configuration,
        IFeatureState state,
        BulletTracerTracker tracker)
    {
        _configuration = configuration;
        _state = state;
        _tracker = tracker;
    }

    public string LayerName => "bullet-tracers";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var options = _configuration.Current.Profile.Visuals.BulletTracers;
        if (!options.Enabled
            || !_state.IsEnabled(FeatureIds.BulletTracers)
            || !snapshot.IsInMatch)
        {
            return [];
        }

        var state = _tracker.CopyState();
        if (state.Tracers.Count == 0)
            return [];

        return BulletTracerDrawBuilder.Build(
            state,
            options,
            projector,
            snapshot.ViewMatrix,
            screenWidth,
            screenHeight,
            ZIndex);
    }
}
