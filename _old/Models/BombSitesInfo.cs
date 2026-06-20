namespace Cs2Toolkit.Models;

public sealed class BombSitesInfo
{
    public Vector3 CenterA { get; init; }
    public Vector3 CenterB { get; init; }

    public bool IsValid => CenterA.IsValid && CenterB.IsValid;

    public static BombSitesInfo Empty { get; } = new();

    public string? LabelForPosition(Vector3 position)
    {
        if (!IsValid || !position.IsValid)
            return null;

        return position.DistanceTo2D(CenterA) <= position.DistanceTo2D(CenterB) ? "A" : "B";
    }
}
