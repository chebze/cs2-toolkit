using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class SoundEspFeatureService : IFeatureService
{
    private readonly IFeatureState _state;
    private readonly SoundEspWaveTracker _tracker;

    public SoundEspFeatureService(IFeatureState state, SoundEspWaveTracker tracker)
    {
        _state = state;
        _tracker = tracker;
    }

    public FeatureId Id => FeatureIds.SoundEsp;

    public bool IsEnabled => true;

    public void OnSnapshot(FeatureContext context)
    {
        if (!_state.IsEnabled(Id))
        {
            _tracker.Update(context.Snapshot, new() { Enabled = false });
            return;
        }

        _tracker.Update(context.Snapshot, context.Settings.Profile.SoundEsp);
    }
}
