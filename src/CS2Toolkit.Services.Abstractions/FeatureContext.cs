using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Input.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services.Abstractions;

public sealed class FeatureContext
{
    public required GameSnapshot Snapshot { get; init; }
    public required ToolkitSettings Settings { get; init; }
    public required ResolvedWeaponSettings WeaponSettings { get; init; }
    public required IInputSimulator Input { get; init; }
}
