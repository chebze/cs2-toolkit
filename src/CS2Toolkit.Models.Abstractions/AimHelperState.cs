namespace CS2Toolkit.Models.Abstractions;

public sealed record AimHelperState(IReadOnlyList<AimCandidate> Candidates)
{
    public static AimHelperState Inactive { get; } = new([]);
}
