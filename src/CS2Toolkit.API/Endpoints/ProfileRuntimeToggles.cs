using CS2Toolkit.Configuration.Abstractions;

namespace CS2Toolkit.API.Endpoints;

internal static class ProfileRuntimeToggles
{
    public static bool Differ(ProfileSettings before, ProfileSettings after) =>
        (before.Triggerbot.Global.Enabled ?? false) != (after.Triggerbot.Global.Enabled ?? false)
        || (before.Rcs.Global.Enabled ?? false) != (after.Rcs.Global.Enabled ?? false)
        || (before.AimHelper.Global.Enabled ?? false) != (after.AimHelper.Global.Enabled ?? false)
        || before.SoundEsp.Enabled != after.SoundEsp.Enabled
        || !string.Equals(before.EnemyEsp.Mode, after.EnemyEsp.Mode, StringComparison.OrdinalIgnoreCase)
        || (before.Triggerbot.Global.AutoStopEnabled ?? false) != (after.Triggerbot.Global.AutoStopEnabled ?? false);
}
