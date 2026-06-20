using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class SoundEspFeatureService : FeatureServiceBase
{
    private readonly SoundEspWaveTracker _tracker;

    public SoundEspFeatureService(IFeatureState state, SoundEspWaveTracker tracker) : base(state, FeatureIds.SoundEsp)
    {
        _tracker = tracker;
    }

    public override void OnSnapshot(FeatureContext context) =>
        _tracker.Update(context.Snapshot, context.Settings.Profile.SoundEsp);
}
