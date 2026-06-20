namespace CS2Toolkit.Configuration.Abstractions;

public sealed class ToolkitHostSettings
{
    public const string SectionName = "Toolkit";

    public int MemoryReadIntervalMs { get; set; } = 5;
    public string DataDirectory { get; set; } = "data";
    public OffsetSettings Offsets { get; set; } = new();
    public MapHostSettings Maps { get; set; } = new();
    public GrenadePhysicsSettings Grenade { get; set; } = new();
    public FileLoggingSettings FileLogging { get; set; } = new();
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
}

public sealed class FileLoggingSettings
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; } = "logs";
}
