namespace CS2Toolkit.Game.Readers;

internal static class MapNameNormalizer
{
    public static string? NormalizeMapName(string? mapName)
    {
        if (string.IsNullOrWhiteSpace(mapName))
            return null;

        var value = mapName.Trim();
        if (value.StartsWith("maps/", StringComparison.OrdinalIgnoreCase))
            value = value["maps/".Length..];

        var dot = value.IndexOf('.');
        if (dot >= 0)
            value = value[..dot];

        return string.IsNullOrWhiteSpace(value) ? null : value.ToLowerInvariant();
    }
}
