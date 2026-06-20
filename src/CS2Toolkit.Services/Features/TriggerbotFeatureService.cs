using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class TriggerbotFeatureService : IFeatureService
{
    private readonly IFeatureState _state;
    private readonly TriggerbotController _controller;

    public TriggerbotFeatureService(IFeatureState state, TriggerbotController controller)
    {
        _state = state;
        _controller = controller;
    }

    public FeatureId Id => FeatureIds.Triggerbot;

    public bool IsEnabled => true;

    public void OnSnapshot(FeatureContext context)
    {
        if (!_state.IsEnabled(Id))
        {
            _controller.Reset(context.Input);
            return;
        }

        var autoStopEnabled = context.WeaponSettings.Triggerbot.AutoStopEnabled
            ?? _state.TriggerbotAutoStopEnabled;

        _controller.Process(context, autoStopEnabled);
    }
}
