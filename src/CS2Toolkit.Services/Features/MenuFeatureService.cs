using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class MenuFeatureService : FeatureServiceBase
{
    public MenuFeatureService(IFeatureState state) : base(state, FeatureIds.Menu)
    {
    }

    public override void OnSnapshot(FeatureContext context)
    {
        // In-game menu overlay arrives in Phase 7.3.12.
    }
}
