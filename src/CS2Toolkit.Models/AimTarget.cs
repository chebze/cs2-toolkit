using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Models;

public sealed record AimTarget(
    PlayerId PlayerId,
    BoneId PreferredBone,
    Vector3 BonePosition,
    float FovDegrees,
    bool HasLineOfSight);
