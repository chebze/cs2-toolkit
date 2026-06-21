namespace CS2Toolkit.Models.Abstractions;

public sealed record BulletImpactEvent(
    PlayerId ShooterId,
    BulletTracerKind Kind,
    Vector3 Start,
    Vector3 End,
    DateTimeOffset Timestamp);
