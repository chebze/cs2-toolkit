using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class AimHelperFeatureService : IFeatureService
{
    private readonly IFeatureState _state;
    private readonly AimHelperController _controller;

    public AimHelperFeatureService(IFeatureState state, AimHelperController controller)
    {
        _state = state;
        _controller = controller;
    }

    public FeatureId Id => FeatureIds.AimHelper;

    public bool IsEnabled => true;

    public void OnSnapshot(FeatureContext context)
    {
        if (!_state.IsEnabled(Id))
            return;

        _controller.Process(context);
    }
}
