using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Drawing.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Services;

public sealed class GrenadeArcOverlayPresenter : IOverlayPresenter
{
    private const int ZIndex = 110;

    private readonly IActiveConfiguration _configuration;
    private readonly GrenadePhysicsSettings _grenadePhysics;

    public GrenadeArcOverlayPresenter(
        IActiveConfiguration configuration,
        IOptions<ToolkitHostSettings> hostSettings)
    {
        _configuration = configuration;
        _grenadePhysics = hostSettings.Value.Grenade;
    }

    public string LayerName => "grenade-arc";

    public IReadOnlyList<DrawCommand> Present(
        GameSnapshot snapshot,
        IWorldProjector projector,
        int screenWidth,
        int screenHeight)
    {
        var options = _configuration.Current.Profile.Visuals.Grenade;
        if (!options.Enabled || !snapshot.IsInMatch)
            return [];

        var grenade = snapshot.Grenades.FirstOrDefault(state => state.IsActive);
        if (grenade is null)
            return [];

        return GrenadeArcDrawBuilder.Build(
            grenade,
            options,
            _grenadePhysics.LandingMarkerRadiusUnits,
            projector,
            snapshot.ViewMatrix,
            screenWidth,
            screenHeight,
            ZIndex);
    }
}
