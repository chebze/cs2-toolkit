namespace Cs2Toolkit.Configuration;

public enum SoundWaveAnimation
{
    Waves,
    StaticBox
}

public sealed class ConfigProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Default";
    public string? SwitchHotkey { get; set; }
    public ProfileSettings Settings { get; set; } = new();
}

public sealed class ProfileSettings
{
    public LayeredWeaponSettings<TriggerbotLayerSettings> Triggerbot { get; set; } = new();
    public LayeredWeaponSettings<RcsLayerSettings> Rcs { get; set; } = new();
    public LayeredWeaponSettings<AimHelperLayerSettings> AimHelper { get; set; } = new();

    public EnemyEspProfileOptions EnemyEsp { get; set; } = new();
    public SoundEspProfileOptions SoundEsp { get; set; } = new();
    public VisualProfileOptions Visuals { get; set; } = new();
}

public sealed class EnemyEspProfileOptions
{
    public string Mode { get; set; } = "LastSeen";
    public bool ShowPlayerName { get; set; }
    public bool ShowPlayerHealth { get; set; }
    public bool ShowBoundingBox { get; set; }
    public string SkeletonColor { get; set; } = "#FF6B6B";
    public float SkeletonLineWidth { get; set; } = 1.5f;
    public string BoundingBoxColor { get; set; } = "#FF6B6B";
}

public sealed class SoundEspProfileOptions
{
    public bool Enabled { get; set; } = true;
    public SoundWaveAnimation Animation { get; set; } = SoundWaveAnimation.Waves;
    public string WaveColor { get; set; } = "#E53935";
    public float WaveLineWidth { get; set; } = 1f;
    public int WaveDurationMs { get; set; } = 900;
    public float MinWorldRadius { get; set; } = 10f;
    public float MaxWorldRadius { get; set; } = 90f;
    public int RingCount { get; set; } = 3;
    public float RingSpacing { get; set; } = 0.22f;
}

public sealed class VisualProfileOptions
{
    public GrenadeVisualOptions Grenade { get; set; } = new();
}

public sealed class GrenadeVisualOptions
{
    public bool Enabled { get; set; } = true;
    public string ArcColor { get; set; } = "#38BDF8";
    public string PointColor { get; set; } = "#38BDF8";
    public string ImpactColor { get; set; } = "#FBBF24";
    public string LandingColor { get; set; } = "#FBBF24";
    public float ArcLineWidth { get; set; } = 2f;
    public float LandingLineWidth { get; set; } = 1.5f;
}

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
}

public sealed class ConfigStore
{
    public string DefaultProfileId { get; set; } = "";
    public string? ActiveProfileId { get; set; }
    public GlobalKeybinds Keybinds { get; set; } = new();
    public List<ConfigProfile> Profiles { get; set; } = [];
    public int WebPort { get; set; } = 8080;
}
