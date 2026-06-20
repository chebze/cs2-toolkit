using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class EnemyEspFeatureService : IFeatureService
{
    private readonly IFeatureState _state;
    private readonly EnemyEspTracker _tracker;

    public EnemyEspFeatureService(IFeatureState state, EnemyEspTracker tracker)
    {
        _state = state;
        _tracker = tracker;
    }

    public FeatureId Id => FeatureIds.EnemyEsp;

    public bool IsEnabled => true;

    public void OnSnapshot(FeatureContext context) =>
        _tracker.Update(context.Snapshot, _state.EnemyEspMode);
}
