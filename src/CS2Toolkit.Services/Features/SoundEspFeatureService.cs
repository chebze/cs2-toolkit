using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class SoundEspFeatureService : FeatureServiceBase
{
    public SoundEspFeatureService(IFeatureState state) : base(state, FeatureIds.SoundEsp)
    {
    }

    public override void OnSnapshot(FeatureContext context)
    {
        // Sound tracking arrives in Phase 7.3.5.
    }
}
