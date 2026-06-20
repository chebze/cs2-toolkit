namespace CS2Toolkit.Services;

internal static class OverlayColorParser
{
    public static uint ParseArgb(string hex, uint fallbackArgb)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallbackArgb;

        var value = hex.Trim();
        if (value.StartsWith('#'))
            value = value[1..];

        if (value.Length is not (6 or 8))
            return fallbackArgb;

        if (!uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var parsed))
            return fallbackArgb;

        return value.Length == 6
            ? 0xFF000000 | parsed
            : parsed;
    }
}
