namespace CS2Toolkit.Configuration.Abstractions;

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
    public float MaxDistanceUnits { get; set; } = 2000f;
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
    public int LandingRingSegments { get; set; } = 20;
}

public sealed class TextPanelOverlayOptions
{
    public bool Enabled { get; set; } = true;
    public int X { get; set; } = 16;
    public int Y { get; set; } = 120;
    public string Color { get; set; } = "#6BCB77";
    public float FontSize { get; set; } = 14f;
}

public sealed class VisualProfileOptions
{
    public GrenadeVisualOptions Grenade { get; set; } = new();
    public TextPanelOverlayOptions TeammateStats { get; set; } = new();
    public TextPanelOverlayOptions BombStatus { get; set; } = new()
    {
        Y = 220,
        Color = "#FFD166"
    };
    public TextPanelOverlayOptions Clairvoyance { get; set; } = new()
    {
        Y = 320,
        Color = "#B794F4"
    };
    public MenuOverlayOptions Menu { get; set; } = new();
    public SystemMessageOverlayOptions SystemMessages { get; set; } = new();
}

public sealed class SystemMessageOverlayOptions
{
    public int Margin { get; set; } = 16;
    public string Color { get; set; } = "#FFFFFFFF";
    public float FontSize { get; set; } = 14f;
}

public sealed class MenuOverlayOptions
{
    public int X { get; set; } = 16;
    public int Y { get; set; } = 16;
    public string BackgroundColor { get; set; } = "#CC1E1E2E";
    public string TextColor { get; set; } = "#FFFFFFFF";
    public float FontSize { get; set; } = 13f;
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

public sealed class ConfigProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Default";
    public string? SwitchHotkey { get; set; }
    public ProfileSettings Settings { get; set; } = new();
}
