using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class EnemyEspOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 100;

    private readonly IActiveConfiguration _configuration;
    private readonly IFeatureState _state;
    private readonly EnemyEspTracker _tracker;

    public EnemyEspOverlayPresenter(
        IActiveConfiguration configuration,
        IFeatureState state,
        EnemyEspTracker tracker)
    {
        _configuration = configuration;
        _state = state;
        _tracker = tracker;
    }

    public string LayerName => "enemy-esp";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var mode = _state.EnemyEspMode;
        if (mode == EnemyEspMode.Disabled || !snapshot.IsInMatch)
            return [];

        var targets = _tracker.CopyDrawableTargets(mode);
        if (targets.Count == 0)
            return [];

        return EnemyEspDrawBuilder.Build(
            targets,
            _configuration.Current.Profile.EnemyEsp,
            projector,
            snapshot.ViewMatrix,
            screenWidth,
            screenHeight,
            ZIndex);
    }
}
