namespace CS2Toolkit.Models.Abstractions;

public readonly record struct PlayerId(int Value)
{
    public override string ToString() => Value.ToString();
}
