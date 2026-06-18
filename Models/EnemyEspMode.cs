namespace Cs2Toolkit.Models;

public enum EnemyEspMode
{
    Disabled = 0,
    LastSeen = 1,
    Full = 2
}

public static class EnemyEspModeParser
{
    public static EnemyEspMode Parse(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "disabled" => EnemyEspMode.Disabled,
            "full" => EnemyEspMode.Full,
            _ => EnemyEspMode.LastSeen
        };

    public static string ToConfigValue(EnemyEspMode mode) => mode switch
    {
        EnemyEspMode.Disabled => "Disabled",
        EnemyEspMode.Full => "Full",
        _ => "LastSeen"
    };
}
