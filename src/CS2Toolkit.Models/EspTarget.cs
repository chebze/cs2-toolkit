using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Models;

public sealed record EspTarget(
    PlayerId PlayerId,
    string Name,
    int Health,
    PlayerBones Bones,
    DateTimeOffset? LastSeenAt);
