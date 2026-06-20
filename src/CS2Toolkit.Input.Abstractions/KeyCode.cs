namespace CS2Toolkit.Input.Abstractions;

public readonly record struct KeyCode(int VirtualKey)
{
    public static KeyCode None { get; } = new(0);

    public bool IsNone => VirtualKey == 0;

    public override string ToString() => VirtualKey.ToString();
}
