namespace CS2Toolkit.Game.Abstractions;

public interface IGameAttachment
{
    bool IsAttached { get; }
    bool TryAttach(string processName = "cs2");
    void Detach();
}
