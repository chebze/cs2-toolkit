using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class RcsFeatureService : FeatureServiceBase
{
    public RcsFeatureService(IFeatureState state) : base(state, FeatureIds.Rcs)
    {
    }

    public override void OnSnapshot(FeatureContext context)
    {
        // Combat logic arrives in Phase 7.3.8.
    }
}
