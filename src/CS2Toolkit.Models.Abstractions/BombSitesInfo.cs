namespace CS2Toolkit.Models.Abstractions;

public sealed record BombSitesInfo(Vector3 CenterA, Vector3 CenterB)
{
    public bool IsValid => CenterA.IsValid && CenterB.IsValid;

    public static BombSitesInfo Empty { get; } = new(default, default);

    public string? LabelForPosition(Vector3 position)
    {
        if (!IsValid || !position.IsValid)
            return null;

        return position.DistanceTo2D(CenterA) <= position.DistanceTo2D(CenterB) ? "A" : "B";
    }
}
