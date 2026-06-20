namespace CS2Toolkit.Drawing.Abstractions;

public sealed record OverlayFrame(
    long Sequence,
    DateTimeOffset ProducedAt,
    IReadOnlyList<DrawCommand> Commands)
{
    public static OverlayFrame Empty { get; } = new(0, DateTimeOffset.MinValue, []);
}
