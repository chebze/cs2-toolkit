using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class TriggerbotFeatureService : FeatureServiceBase
{
    public TriggerbotFeatureService(IFeatureState state) : base(state, FeatureIds.Triggerbot)
    {
    }

    public override void OnSnapshot(FeatureContext context)
    {
        // Combat logic arrives in Phase 7.3.7.
    }
}
