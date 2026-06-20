using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services;

public sealed class SoundEspOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 200;

    private readonly IActiveConfiguration _configuration;
    private readonly IFeatureState _state;
    private readonly SoundEspWaveTracker _tracker;

    public SoundEspOverlayPresenter(
        IActiveConfiguration configuration,
        IFeatureState state,
        SoundEspWaveTracker tracker)
    {
        _configuration = configuration;
        _state = state;
        _tracker = tracker;
    }

    public string LayerName => "sound-esp";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var options = _configuration.Current.Profile.SoundEsp;
        if (!options.Enabled
            || !_state.IsEnabled(FeatureIds.SoundEsp)
            || !snapshot.IsInMatch)
        {
            return [];
        }

        var state = _tracker.CopyState();
        if (state.Waves.Count == 0 && state.BombPosition is not { IsValid: true })
            return [];

        return SoundEspDrawBuilder.Build(
            state,
            options,
            projector,
            snapshot.ViewMatrix,
            screenWidth,
            screenHeight,
            ZIndex);
    }
}
