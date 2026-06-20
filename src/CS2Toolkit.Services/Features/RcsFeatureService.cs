using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class RcsFeatureService : IFeatureService
{
    private readonly IFeatureState _state;
    private readonly RcsController _controller;

    public RcsFeatureService(IFeatureState state, RcsController controller)
    {
        _state = state;
        _controller = controller;
    }

    public FeatureId Id => FeatureIds.Rcs;

    public bool IsEnabled => true;

    public void OnSnapshot(FeatureContext context)
    {
        if (!_state.IsEnabled(Id))
        {
            _controller.Reset();
            return;
        }

        _controller.Process(context);
    }
}
