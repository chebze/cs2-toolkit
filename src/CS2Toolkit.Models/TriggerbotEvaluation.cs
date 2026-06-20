using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Models;

public sealed record TriggerbotEvaluation(
    bool ShouldFire,
    bool IsOnTarget,
    bool PassedLineOfSight,
    int ReactionDelayMs,
    PlayerId? TargetPlayerId);
