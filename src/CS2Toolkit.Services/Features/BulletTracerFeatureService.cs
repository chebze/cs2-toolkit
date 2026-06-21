using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class BulletTracerFeatureService : IFeatureService
{
    private readonly IFeatureState _state;
    private readonly BulletTracerTracker _tracker;

    public BulletTracerFeatureService(IFeatureState state, BulletTracerTracker tracker)
    {
        _state = state;
        _tracker = tracker;
    }

    public FeatureId Id => FeatureIds.BulletTracers;

    public bool IsEnabled => true;

    public void OnSnapshot(FeatureContext context)
    {
        if (!_state.IsEnabled(Id))
        {
            _tracker.Update(context.Snapshot, new() { Enabled = false });
            return;
        }

        _tracker.Update(context.Snapshot, context.Settings.Profile.Visuals.BulletTracers);
    }
}
