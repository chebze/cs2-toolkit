using CS2Toolkit.Services.Abstractions;

namespace CS2Toolkit.Services.Features;

internal sealed class MenuFeatureService : IFeatureService
{
    public FeatureId Id => FeatureIds.Menu;

    public bool IsEnabled => true;

    public void OnSnapshot(FeatureContext context)
    {
        // Menu rendering and interactivity are handled by MenuOverlayPresenter and OverlayFrame.Interactive.
    }
}
