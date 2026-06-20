namespace CS2Toolkit.Models.Abstractions;

public interface IReadOnlyGameState
{
    GameSnapshot? Latest { get; }
}
