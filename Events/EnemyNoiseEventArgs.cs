using Cs2Toolkit.Models;

namespace Cs2Toolkit.Events;

public sealed class EnemyNoiseEventArgs : EventArgs
{
    public int PlayerIndex { get; init; }
    public EnemySoundType SoundType { get; init; }
    public Vector3 WorldPosition { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
