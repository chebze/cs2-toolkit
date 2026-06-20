using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

internal static class BombStatusFormatter
{
    public static IReadOnlyList<string> BuildLines(BombState bomb)
    {
        var lines = new List<string> { "Bomb", $"  {FormatStatus(bomb.Status)}" };

        if (bomb.Status is BombStatus.Planting or BombStatus.Planted && !string.IsNullOrEmpty(bomb.Site))
            lines.Add($"  Site: {bomb.Site}");

        if (bomb.Status == BombStatus.Planted && bomb.TimeLeftSeconds is not null)
            lines.Add($"  Time left: {bomb.TimeLeftSeconds}s");

        if (bomb.Status == BombStatus.Defusing)
        {
            if (bomb.TimeLeftSeconds is not null)
                lines.Add($"  Time left: {bomb.TimeLeftSeconds}s");

            lines.Add($"  Kit: {FormatYesNo(bomb.HasDefuseKit)}");

            if (bomb.DefuseTimeSeconds is not null)
                lines.Add($"  Time to defuse: {bomb.DefuseTimeSeconds}s");

            lines.Add($"  Will succeed: {FormatYesNo(bomb.WillDefuseSucceed)}");
        }

        return lines;
    }

    private static string FormatStatus(BombStatus status) => status switch
    {
        BombStatus.Carried => "Carried",
        BombStatus.Equipped => "Equipped",
        BombStatus.OnGround => "On ground",
        BombStatus.Defusing => "Defusing",
        BombStatus.Planting => "Planting",
        BombStatus.Planted => "Planted",
        _ => string.Empty
    };

    private static string FormatYesNo(bool? value) => value switch
    {
        true => "yes",
        false => "no",
        _ => "unknown"
    };
}
