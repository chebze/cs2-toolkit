namespace Cs2Toolkit.Configuration;

public sealed class ToolkitOptions
{
    public const string SectionName = "Toolkit";

    public string InjectKey { get; set; } = "F9";
    public string MenuToggleKey { get; set; } = "Insert";
    public string PanicKey { get; set; } = "F10";
    public string SaveSettingsKey { get; set; } = "F11";
    public int MemoryReadIntervalMs { get; set; } = 5;
    public OffsetOptions Offsets { get; set; } = new();
    public OverlayOptions Overlay { get; set; } = new();
    public RcsOptions Rcs { get; set; } = new();
    public TbOptions Tb { get; set; } = new();
    public EnemyEspOptions EnemyEsp { get; set; } = new();
    public SoundEspOptions SoundEsp { get; set; } = new();
    public AimHelperOptions AimHelper { get; set; } = new();
    public ClairvoyanceOptions Clairvoyance { get; set; } = new();
    public EnemyNoiseOptions EnemyNoise { get; set; } = new();
    public MapOptions Maps { get; set; } = new();
    public GrenadeOptions Grenade { get; set; } = new();
    public FileLoggingOptions FileLogging { get; set; } = new();
}

public sealed class GrenadeOptions
{
    public float TickIntervalSeconds { get; set; } = 1f / 64f;
    public float Gravity { get; set; } = 320f;
    public float BounceElasticity { get; set; } = 0.45f;
    public float SurfaceOffset { get; set; } = 0.25f;
    public float StopVelocityThreshold { get; set; } = 20f;
    public float MinThrowSpeedScale { get; set; } = 0.7f;
    public float MaxThrowSpeedScale { get; set; } = 0.3f;
    public float PlayerVelocityScale { get; set; } = 1.25f;
    public int MaxSimulationTicks { get; set; } = 512;
    public int MaxBounces { get; set; } = 10;
    public int MaxTrailPoints { get; set; } = 500;
    public int MaxEntityScanIndex { get; set; } = 1024;
    public int MinGameTrailPoints { get; set; } = 8;
    public int RaycastSubSteps { get; set; } = 4;
    public float RaycastSkin { get; set; } = 0.5f;
    public int RecordIntervalTicks { get; set; } = 1;
    public float MinPointSpacingUnits { get; set; } = 4f;
    public float MinTrajectoryHorizontalTravelUnits { get; set; } = 32f;
    public float ThrowForwardTraceUnits { get; set; } = 22f;
    public float ThrowStartPullbackUnits { get; set; } = 6f;
    public float LandingMarkerRadiusUnits { get; set; } = 18f;
}

public sealed class MapOptions
{
    public string CacheDirectory { get; set; } = "data/maps";
    public string? MapsDirectory { get; set; }
}

public sealed class EnemyNoiseOptions
{
    public float MaxDistanceUnits { get; set; } = 2000f;
    public int WaveDurationMs { get; set; } = 900;
    public float MinWorldRadius { get; set; } = 10f;
    public float MaxWorldRadius { get; set; } = 90f;
    public int RingCount { get; set; } = 3;
    public float RingSpacing { get; set; } = 0.22f;
    public float WaveLineWidth { get; set; } = 1f;
    public string WaveColor { get; set; } = "#E53935";
}

public sealed class RcsOptions
{
    public string ToggleKey { get; set; } = "F8";
    public bool Enabled { get; set; }
    public float Sensitivity { get; set; } = 1.25f;
    public float PitchScale { get; set; } = 2f;
    public float YawScale { get; set; } = 2f;
    public float FirstBulletCompensateChance { get; set; } = 0.5f;
    public float SubsequentBulletSkipChance { get; set; } = 0.2f;
}

public sealed class TbOptions
{
    public string ToggleKey { get; set; } = "F7";
    public bool Enabled { get; set; }
    public bool AutoStopEnabled { get; set; }
    public string AutoStrafeKey { get; set; } = "Space";
    public float AutoStopSpeedThreshold { get; set; } = 15f;
    public float PreFireFovDegrees { get; set; } = 0.7f;
    public float MinPreFireFovDegrees { get; set; } = 0.1f;
    public float MaxPreFireFovDegrees { get; set; } = 5f;
    public float FovAdjustStepDegrees { get; set; } = 0.05f;
    public int FovAdjustRepeatIntervalMs { get; set; } = 80;
    public int MinGraceBullets { get; set; } = 1;
    public int MaxGraceBullets { get; set; } = 2;
    public int MinReactionDelayMs { get; set; } = 200;
    public int MaxReactionDelayMs { get; set; } = 400;
    public int ReactionDelayAdjustStepMs { get; set; } = 50;
    public string FovCircleColor { get; set; } = "#EF4444";
    public float FovCircleLineWidth { get; set; } = 1.5f;
    public float AssumedHorizontalFovDegrees { get; set; } = 90f;
}

public sealed class EnemyEspOptions
{
    public string ToggleKey { get; set; } = "F6";
    public string Mode { get; set; } = "LastSeen";
}

public sealed class SoundEspOptions
{
    public string ToggleKey { get; set; } = "F5";
    public bool Enabled { get; set; } = true;
}

public sealed class AimHelperOptions
{
    public string ToggleKey { get; set; } = "F4";
    public bool Enabled { get; set; }
    public string ActivationKey { get; set; } = "";
    public string PreferredBone { get; set; } = "Head";
    public float FovDegrees { get; set; } = 3f;
    public float MinFovDegrees { get; set; } = 0.5f;
    public float MaxFovDegrees { get; set; } = 15f;
    public float FovAdjustStepDegrees { get; set; } = 0.25f;
    public int FovAdjustRepeatIntervalMs { get; set; } = 80;
    public string FovCircleColor { get; set; } = "#38BDF8";
    public float FovCircleLineWidth { get; set; } = 1.5f;
    public float AssumedHorizontalFovDegrees { get; set; } = 90f;
}

public sealed class ClairvoyanceOptions
{
    public int LowAmmoClipThreshold { get; set; } = 3;
    public float EnemyCloseDistanceUnits { get; set; } = 600f;
    public float BombsiteEnemyRadiusUnits { get; set; } = 1500f;
}

public sealed class FileLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "logs";
    public string FileNamePrefix { get; set; } = "cs2-toolkit";
    public bool LogStatChanges { get; set; } = true;
    public bool LogPlayerDetailsOnRoundEvents { get; set; } = true;
}

public sealed class OffsetOptions
{
    public string OffsetsUrl { get; set; } =
        "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.json";

    public string ClientDllUrl { get; set; } =
        "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.json";
}

public sealed class OverlayOptions
{
    public int TargetFps { get; set; }
    public SkeletonOverlayOptions EnemyLastSeen { get; set; } = new() { Color = "#FF6B6B", LineWidth = 1.5f };
    public TextPanelOptions TeammateStats { get; set; } = new() { X = 16, Y = 120, Color = "#6BCB77", FontSize = 14 };
    public TextPanelOptions BombStatus { get; set; } = new() { X = 16, Y = 220, Color = "#FFD166", FontSize = 14 };
    public TextPanelOptions Clairvoyance { get; set; } = new() { X = 16, Y = 320, Color = "#B794F4", FontSize = 14 };
    public MenuPanelOptions Menu { get; set; } = new();
    public TextPanelOptions InjectionPrompt { get; set; } = new() { Color = "#FFFFFFFF", FontSize = 14 };
    public RcsStatusPanelOptions RcsStatus { get; set; } = new();
    public RcsStatusPanelOptions TbStatus { get; set; } = new();
    public EspStatusPanelOptions EspStatus { get; set; } = new();
    public RcsStatusPanelOptions SoundEspStatus { get; set; } = new();
    public RcsStatusPanelOptions AimHelperStatus { get; set; } = new();
    public GrenadeOverlayOptions GrenadeTrajectory { get; set; } = new();
}

public sealed class EspStatusPanelOptions
{
    public int FontSize { get; set; } = 16;
    public int Margin { get; set; } = 16;
    public string DisabledColor { get; set; } = "#EF4444";
    public string LastSeenColor { get; set; } = "#F59E0B";
    public string FullColor { get; set; } = "#22C55E";
}

public sealed class GrenadeOverlayOptions
{
    public bool Enabled { get; set; } = true;
    public string ArcColor { get; set; } = "#38BDF8";
    public string LandingColor { get; set; } = "#FBBF24";
    public float ArcLineWidth { get; set; } = 2f;
    public float LandingLineWidth { get; set; } = 1.5f;
    public int LandingRingSegments { get; set; } = 20;
    public bool LogDiagnostics { get; set; } = true;
    public int LogDiagnosticsIntervalMs { get; set; } = 2000;
}

public sealed class RcsStatusPanelOptions
{
    public int FontSize { get; set; } = 16;
    public int Margin { get; set; } = 16;
    public string EnabledColor { get; set; } = "#22C55E";
    public string DisabledColor { get; set; } = "#EF4444";
}

public sealed class SkeletonOverlayOptions
{
    public string Color { get; set; } = "#FF6B6B";
    public float LineWidth { get; set; } = 1.5f;
}

public sealed class TextPanelOptions
{
    public bool Enabled { get; set; } = true;
    public int X { get; set; }
    public int Y { get; set; }
    public string Color { get; set; } = "#FFFFFFFF";
    public int FontSize { get; set; } = 14;
}

public sealed class MenuPanelOptions
{
    public int X { get; set; } = 16;
    public int Y { get; set; } = 16;
    public string BackgroundColor { get; set; } = "#CC1E1E2E";
    public string TextColor { get; set; } = "#FFFFFFFF";
    public int FontSize { get; set; } = 13;
}
