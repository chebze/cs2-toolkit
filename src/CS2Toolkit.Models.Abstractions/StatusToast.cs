namespace CS2Toolkit.Models.Abstractions;

public sealed record StatusToast(
    string Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    uint ColorArgb);
