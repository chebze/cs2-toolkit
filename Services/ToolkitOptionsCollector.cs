using System.Text.Json;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Services;

public static class ToolkitOptionsCollector
{
    private static readonly JsonSerializerOptions CloneOptions = new();

    public static ToolkitOptions Collect(
        ToolkitOptions config,
        RcsState rcsState,
        TbState tbState,
        EnemyEspState enemyEspState,
        SoundEspState soundEspState,
        AimHelperState aimHelperState)
    {
        var options = Clone(config);

        options.Rcs.Enabled = rcsState.IsEnabled;

        options.Tb.Enabled = tbState.IsEnabled;
        options.Tb.AutoStopEnabled = tbState.IsAutoStopEnabled;
        options.Tb.PreFireFovDegrees = tbState.PreFireFovDegrees;
        options.Tb.MinReactionDelayMs = tbState.MinReactionDelayMs;
        options.Tb.MaxReactionDelayMs = tbState.MaxReactionDelayMs;

        options.EnemyEsp.Mode = EnemyEspModeParser.ToConfigValue(enemyEspState.Mode);

        options.SoundEsp.Enabled = soundEspState.IsEnabled;

        options.AimHelper.Enabled = aimHelperState.IsEnabled;
        options.AimHelper.FovDegrees = aimHelperState.FovDegrees;

        return options;
    }

    private static ToolkitOptions Clone(ToolkitOptions source)
    {
        var json = JsonSerializer.Serialize(source, CloneOptions);
        return JsonSerializer.Deserialize<ToolkitOptions>(json)
            ?? throw new InvalidOperationException("Failed to clone ToolkitOptions.");
    }
}
