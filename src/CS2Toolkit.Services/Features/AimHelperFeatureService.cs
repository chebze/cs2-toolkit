using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class AimHelperFeatureService : FeatureServiceBase
{
    public AimHelperFeatureService(IFeatureState state) : base(state, FeatureIds.AimHelper)
    {
    }

    public override void OnSnapshot(FeatureContext context)
    {
        // Aim logic arrives in Phase 7.3.9.
    }
}
