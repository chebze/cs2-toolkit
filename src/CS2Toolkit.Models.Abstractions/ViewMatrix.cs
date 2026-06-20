namespace CS2Toolkit.Models.Abstractions;

public readonly struct ViewMatrix(ReadOnlySpan<float> values)
{
    public const int FloatCount = 16;

    private readonly float[] _values = values.Length == FloatCount ? values.ToArray() : new float[FloatCount];

    public ReadOnlySpan<float> Values => _values;

    public float this[int index] => _values[index];
}
