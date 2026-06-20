namespace CS2Toolkit.Game.Abstractions;

public sealed record OffsetMetadata(
    string Source,
    DateTimeOffset RetrievedAt,
    bool IsValid);

public interface IOffsetProvider
{
    OffsetMetadata Metadata { get; }
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);
}
