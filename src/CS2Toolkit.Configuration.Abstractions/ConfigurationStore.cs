namespace CS2Toolkit.Configuration.Abstractions;

public sealed class GlobalKeybinds
{
    public string InjectKey { get; set; } = "F9";
    public string MenuToggleKey { get; set; } = "Insert";
    public string PanicKey { get; set; } = "F10";
    public string SaveSettingsKey { get; set; } = "F11";
    public string RcsToggleKey { get; set; } = "F8";
    public string TbToggleKey { get; set; } = "F7";
    public string EnemyEspToggleKey { get; set; } = "F6";
    public string SoundEspToggleKey { get; set; } = "F5";
    public string AimHelperToggleKey { get; set; } = "F4";
    public string AimHelperActivationKey { get; set; } = "";
    public string TbAutoStrafeKey { get; set; } = "Space";
    public string BulletTracersToggleKey { get; set; } = "F3";
}

public sealed class ConfigurationStore
{
    public string DefaultProfileId { get; set; } = "";
    public string? ActiveProfileId { get; set; }
    public GlobalKeybinds Keybinds { get; set; } = new();
    public List<ConfigProfile> Profiles { get; set; } = [];
    public int WebPort { get; set; } = 8080;
}
