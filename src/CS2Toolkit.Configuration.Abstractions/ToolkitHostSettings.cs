namespace CS2Toolkit.Configuration.Abstractions;

public sealed class ClairvoyanceSettings
{
    public int LowAmmoClipThreshold { get; set; } = 3;
    public float EnemyCloseDistanceUnits { get; set; } = 600f;
    public float BombsiteEnemyRadiusUnits { get; set; } = 1500f;
}

public sealed class ToolkitHostSettings
{
    public const string SectionName = "Toolkit";

    public int MemoryReadIntervalMs { get; set; } = 5;
    public string DataDirectory { get; set; } = "data";
    public OffsetSettings Offsets { get; set; } = new();
    public MapHostSettings Maps { get; set; } = new();
    public GrenadePhysicsSettings Grenade { get; set; } = new();
    public ClairvoyanceSettings Clairvoyance { get; set; } = new();
    public TriggerbotHostSettings Triggerbot { get; set; } = new();
    public RcsHostSettings Rcs { get; set; } = new();
    public AimHelperHostSettings AimHelper { get; set; } = new();
    public FileLoggingSettings FileLogging { get; set; } = new();
    public bool OpenConfigUiOnStart { get; set; } = true;
    public bool ShowDebugPlayerBoxes { get; set; }
}

public sealed class AimHelperHostSettings
{
    public int StatusFontSize { get; set; } = 16;
    public int StatusMargin { get; set; } = 16;
    public string EnabledColor { get; set; } = "#22C55E";
    public string DisabledColor { get; set; } = "#EF4444";
    public string FovCircleColor { get; set; } = "#38BDF8";
    public float FovCircleLineWidth { get; set; } = 1.5f;
    public float AssumedHorizontalFovDegrees { get; set; } = 90f;
}

public sealed class RcsHostSettings
{
    public int StatusFontSize { get; set; } = 16;
    public int StatusMargin { get; set; } = 16;
    public string EnabledColor { get; set; } = "#22C55E";
    public string DisabledColor { get; set; } = "#EF4444";
}

public sealed class TriggerbotHostSettings
{
    public float AutoStopSpeedThreshold { get; set; } = 15f;
    public int MinGraceBullets { get; set; } = 1;
    public int MaxGraceBullets { get; set; } = 2;
    public float AssumedHorizontalFovDegrees { get; set; } = 90f;
    public string FovCircleColor { get; set; } = "#EF4444";
    public float FovCircleLineWidth { get; set; } = 1.5f;
    public int StatusFontSize { get; set; } = 16;
    public int StatusMargin { get; set; } = 16;
}

public sealed class OffsetSettings
{
    public string OffsetsUrl { get; set; } =
        "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.json";

    public string ClientDllUrl { get; set; } =
        "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.json";
}

public sealed class MapHostSettings
{
    public string CacheDirectory { get; set; } = "data/maps";
    public string? MapsDirectory { get; set; }
}

public sealed class GrenadePhysicsSettings
{
    public float TickIntervalSeconds { get; set; } = 1f / 64f;
    public float Gravity { get; set; } = 320f;
    public float BounceElasticity { get; set; } = 0.45f;
    public float SurfaceOffset { get; set; } = 0.25f;
    public float StopVelocityThreshold { get; set; } = 20f;
    public int MaxSimulationTicks { get; set; } = 512;
    public int MaxBounces { get; set; } = 10;
    public float MinThrowSpeedScale { get; set; } = 0.7f;
    public float MaxThrowSpeedScale { get; set; } = 0.3f;
    public float PlayerVelocityScale { get; set; } = 1.25f;
    public int RaycastSubSteps { get; set; } = 4;
    public float RaycastSkin { get; set; } = 0.5f;
    public float MinPointSpacingUnits { get; set; } = 4f;
    public float ThrowForwardTraceUnits { get; set; } = 22f;
    public float ThrowStartPullbackUnits { get; set; } = 6f;
    public int MaxEntityScanIndex { get; set; } = 1024;
    public float LandingMarkerRadiusUnits { get; set; } = 18f;
}

public sealed class FileLoggingSettings
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "logs";
}
