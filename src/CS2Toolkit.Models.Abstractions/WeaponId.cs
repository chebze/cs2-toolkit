namespace CS2Toolkit.Models.Abstractions;

public readonly record struct WeaponId(ushort Value)
{
    public override string ToString() => Value.ToString();
}
