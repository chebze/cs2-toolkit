namespace CS2Toolkit.Input.Abstractions;

public sealed class KeybindActivatedEventArgs : EventArgs
{
    public required KeybindMatch Match { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

public interface IKeybindDispatcher
{
    event EventHandler<KeybindActivatedEventArgs>? KeybindActivated;
}
