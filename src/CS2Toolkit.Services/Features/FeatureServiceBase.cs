using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal abstract class FeatureServiceBase : IFeatureService
{
    private readonly IFeatureState _state;

    protected FeatureServiceBase(IFeatureState state, FeatureId id)
    {
        _state = state;
        Id = id;
    }

    public FeatureId Id { get; }

    public bool IsEnabled => _state.IsEnabled(Id);

    public abstract void OnSnapshot(FeatureContext context);
}
