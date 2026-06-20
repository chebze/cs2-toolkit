namespace CS2Toolkit.Configuration.Abstractions;

/// <summary>
/// Flattened runtime settings built from the active profile, keybinds, and host settings.
/// </summary>
public sealed class ToolkitSettings
{
    public GlobalKeybinds Keybinds { get; init; } = new();
    public ToolkitHostSettings Host { get; init; } = new();
    public ProfileSettings Profile { get; init; } = new();
    public int WebPort { get; init; } = 8080;
    public string ActiveProfileId { get; init; } = "";
    public string ActiveProfileName { get; init; } = "Default";
}

public sealed class ResolvedWeaponSettings
{
    public TriggerbotLayerSettings Triggerbot { get; init; } = new();
    public RcsLayerSettings Rcs { get; init; } = new();
    public AimHelperLayerSettings AimHelper { get; init; } = new();
}
