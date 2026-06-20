namespace CS2Toolkit.Models.Abstractions;

public interface IGameStateSource
{
    IAsyncEnumerable<GameSnapshot> WatchAsync(CancellationToken cancellationToken = default);
}
