namespace CS2Toolkit.Runtime.Abstractions;

/// <summary>
/// Ordered startup phases for the toolkit host.
/// Phase 9 will add concrete orchestration gates.
/// </summary>
public enum StartupPhase
{
    Offsets,
    Maps,
    Overlay,
    Attach,
    GameLoop,
    Input,
    Features,
    Api
}
