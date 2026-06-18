namespace Cs2Toolkit.Models;

public sealed class GrenadeTrajectoryDiagnostics
{
    public GrenadeTrajectorySnapshot Snapshot { get; init; } = new();
    public string Status { get; init; } = "inactive";
}
