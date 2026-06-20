namespace CS2Toolkit.Models.Abstractions;

public readonly struct Vector3(float x, float y, float z)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Z { get; } = z;

    public bool IsValid =>
        !float.IsNaN(X) && !float.IsNaN(Y) && !float.IsNaN(Z)
        && (MathF.Abs(X) > 1f || MathF.Abs(Y) > 1f || MathF.Abs(Z) > 1f);

    public float DistanceTo(Vector3 other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public float DistanceTo2D(Vector3 other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
