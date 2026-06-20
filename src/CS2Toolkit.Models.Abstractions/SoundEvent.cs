namespace CS2Toolkit.Models.Abstractions;

public sealed record SoundEvent(
    PlayerId PlayerId,
    SoundKind Kind,
    Vector3 Position,
    DateTimeOffset Timestamp);
