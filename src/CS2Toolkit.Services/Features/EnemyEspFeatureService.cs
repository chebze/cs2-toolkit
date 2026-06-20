using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class EnemyEspFeatureService : FeatureServiceBase
{
    private readonly IFeatureState _state;
    private readonly EnemyEspTracker _tracker;

    public EnemyEspFeatureService(IFeatureState state, EnemyEspTracker tracker) : base(state, FeatureIds.EnemyEsp)
    {
        _state = state;
        _tracker = tracker;
    }

    public override void OnSnapshot(FeatureContext context) =>
        _tracker.Update(context.Snapshot, _state.EnemyEspMode);
}
