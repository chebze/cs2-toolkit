namespace CS2Toolkit.Models.Abstractions;

public sealed record AimCandidate(
    PlayerId PlayerId,
    BoneId Bone,
    Vector3 BonePosition,
    float AngularDistanceDegrees);
