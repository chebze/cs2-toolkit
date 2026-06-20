namespace CS2Toolkit.Game.Abstractions;

public enum GameLifecycleState
{
    WaitingForOffsets,
    WaitingForGame,
    WaitingForAttach,
    Attached,
    Failed
}

public interface IGameLifecycle
{
    GameLifecycleState State { get; }
    event Action<GameLifecycleState>? StateChanged;
}
