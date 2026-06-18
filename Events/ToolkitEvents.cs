using Cs2Toolkit.Models;

namespace Cs2Toolkit.Events;

public sealed class KeyInputEventArgs : EventArgs
{
    public Keys Key { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public sealed class MouseInputEventArgs : EventArgs
{
    public MouseButtons Button { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public sealed class MemoryReadEventArgs : EventArgs
{
    public MemoryState State { get; init; } = MemoryState.Detached;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public enum InjectionStatus
{
    WaitingForGame,
    WaitingForKeyPress,
    Attaching,
    Attached,
    Failed
}

public sealed class InjectionStatusEventArgs : EventArgs
{
    public InjectionStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
}
