using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class EnemyEspFeatureService : FeatureServiceBase
{
    public EnemyEspFeatureService(IFeatureState state) : base(state, FeatureIds.EnemyEsp)
    {
    }

    public override void OnSnapshot(FeatureContext context)
    {
        // Overlay + tracking arrives in Phase 7.3.4.
    }
}
