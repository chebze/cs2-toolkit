using System.Text.Json;
using System.Text.Json.Nodes;
using CS2Toolkit.Configuration.Abstractions;
using Microsoft.Extensions.Hosting;

namespace CS2Toolkit.Configuration;

public sealed class LegacySettingsMigrator
{
    private const string LegacySectionName = "Toolkit";

    public ConfigurationStore MigrateFromLegacyAppSettings(IHostEnvironment environment)
    {
        var appSettingsPath = Path.Combine(environment.ContentRootPath, "appsettings.json");
        var legacyPath = Path.Combine(environment.ContentRootPath, "_old", "appsettings.json");

        var legacy = ReadLegacyOptions(File.Exists(appSettingsPath) ? appSettingsPath : legacyPath);
        var profile = MapFromLegacyOptions(legacy, "Default");

        return new ConfigurationStore
        {
            DefaultProfileId = profile.Id,
            ActiveProfileId = profile.Id,
            Keybinds = MapKeybinds(legacy),
            Profiles = [profile],
            WebPort = 8080
        };
    }

    private static LegacyToolkitOptions ReadLegacyOptions(string path)
    {
        if (!File.Exists(path))
            return new LegacyToolkitOptions();

        var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
        if (root?[LegacySectionName] is not JsonNode toolkitNode)
            return new LegacyToolkitOptions();

        return toolkitNode.Deserialize<LegacyToolkitOptions>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new LegacyToolkitOptions();
    }

    private static ConfigProfile MapFromLegacyOptions(LegacyToolkitOptions options, string name)
    {
        var profile = new ConfigProfile { Name = name };
        var s = profile.Settings;

        s.Triggerbot.Global = new TriggerbotLayerSettings
        {
            Enabled = options.Tb?.Enabled,
            AutoStopEnabled = options.Tb?.AutoStopEnabled,
            PreFireFovDegrees = options.Tb?.PreFireFovDegrees,
            MinReactionDelayMs = options.Tb?.MinReactionDelayMs,
            MaxReactionDelayMs = options.Tb?.MaxReactionDelayMs
        };

        s.Rcs.Global = new RcsLayerSettings
        {
            Enabled = options.Rcs?.Enabled,
            Sensitivity = options.Rcs?.Sensitivity,
            PitchScale = options.Rcs?.PitchScale,
            YawScale = options.Rcs?.YawScale,
            FirstBulletCompensateChance = options.Rcs?.FirstBulletCompensateChance,
            SubsequentBulletSkipChance = options.Rcs?.SubsequentBulletSkipChance
        };

        s.AimHelper.Global = new AimHelperLayerSettings
        {
            Enabled = options.AimHelper?.Enabled,
            PreferredBone = options.AimHelper?.PreferredBone,
            FovDegrees = options.AimHelper?.FovDegrees
        };

        if (options.EnemyEsp?.Mode is not null)
            s.EnemyEsp.Mode = options.EnemyEsp.Mode;

        if (options.Overlay?.EnemyLastSeen is not null)
        {
            s.EnemyEsp.SkeletonColor = options.Overlay.EnemyLastSeen.Color ?? s.EnemyEsp.SkeletonColor;
            s.EnemyEsp.SkeletonLineWidth = options.Overlay.EnemyLastSeen.LineWidth ?? s.EnemyEsp.SkeletonLineWidth;
        }

        if (options.SoundEsp?.Enabled is not null)
            s.SoundEsp.Enabled = options.SoundEsp.Enabled.Value;

        if (options.EnemyNoise is not null)
        {
            s.SoundEsp.WaveColor = options.EnemyNoise.WaveColor ?? s.SoundEsp.WaveColor;
            s.SoundEsp.WaveLineWidth = options.EnemyNoise.WaveLineWidth ?? s.SoundEsp.WaveLineWidth;
            s.SoundEsp.WaveDurationMs = options.EnemyNoise.WaveDurationMs ?? s.SoundEsp.WaveDurationMs;
            s.SoundEsp.MinWorldRadius = options.EnemyNoise.MinWorldRadius ?? s.SoundEsp.MinWorldRadius;
            s.SoundEsp.MaxWorldRadius = options.EnemyNoise.MaxWorldRadius ?? s.SoundEsp.MaxWorldRadius;
            s.SoundEsp.RingCount = options.EnemyNoise.RingCount ?? s.SoundEsp.RingCount;
            s.SoundEsp.RingSpacing = options.EnemyNoise.RingSpacing ?? s.SoundEsp.RingSpacing;
            s.SoundEsp.MaxDistanceUnits = options.EnemyNoise.MaxDistanceUnits ?? s.SoundEsp.MaxDistanceUnits;
        }

        if (options.Overlay?.GrenadeTrajectory is not null)
        {
            s.Visuals.Grenade.Enabled = options.Overlay.GrenadeTrajectory.Enabled ?? s.Visuals.Grenade.Enabled;
            s.Visuals.Grenade.ArcColor = options.Overlay.GrenadeTrajectory.ArcColor ?? s.Visuals.Grenade.ArcColor;
            s.Visuals.Grenade.LandingColor = options.Overlay.GrenadeTrajectory.LandingColor ?? s.Visuals.Grenade.LandingColor;
            s.Visuals.Grenade.ArcLineWidth = options.Overlay.GrenadeTrajectory.ArcLineWidth ?? s.Visuals.Grenade.ArcLineWidth;
            s.Visuals.Grenade.LandingLineWidth = options.Overlay.GrenadeTrajectory.LandingLineWidth ?? s.Visuals.Grenade.LandingLineWidth;
        }

        if (options.Overlay?.TeammateStats is not null)
        {
            var teammate = options.Overlay.TeammateStats;
            s.Visuals.TeammateStats.Enabled = teammate.Enabled ?? s.Visuals.TeammateStats.Enabled;
            s.Visuals.TeammateStats.X = teammate.X ?? s.Visuals.TeammateStats.X;
            s.Visuals.TeammateStats.Y = teammate.Y ?? s.Visuals.TeammateStats.Y;
            s.Visuals.TeammateStats.Color = teammate.Color ?? s.Visuals.TeammateStats.Color;
            s.Visuals.TeammateStats.FontSize = teammate.FontSize ?? s.Visuals.TeammateStats.FontSize;
        }

        if (options.Overlay?.BombStatus is not null)
        {
            var bomb = options.Overlay.BombStatus;
            s.Visuals.BombStatus.Enabled = bomb.Enabled ?? s.Visuals.BombStatus.Enabled;
            s.Visuals.BombStatus.X = bomb.X ?? s.Visuals.BombStatus.X;
            s.Visuals.BombStatus.Y = bomb.Y ?? s.Visuals.BombStatus.Y;
            s.Visuals.BombStatus.Color = bomb.Color ?? s.Visuals.BombStatus.Color;
            s.Visuals.BombStatus.FontSize = bomb.FontSize ?? s.Visuals.BombStatus.FontSize;
        }

        if (options.Overlay?.Clairvoyance is not null)
        {
            var clairvoyance = options.Overlay.Clairvoyance;
            s.Visuals.Clairvoyance.Enabled = clairvoyance.Enabled ?? s.Visuals.Clairvoyance.Enabled;
            s.Visuals.Clairvoyance.X = clairvoyance.X ?? s.Visuals.Clairvoyance.X;
            s.Visuals.Clairvoyance.Y = clairvoyance.Y ?? s.Visuals.Clairvoyance.Y;
            s.Visuals.Clairvoyance.Color = clairvoyance.Color ?? s.Visuals.Clairvoyance.Color;
            s.Visuals.Clairvoyance.FontSize = clairvoyance.FontSize ?? s.Visuals.Clairvoyance.FontSize;
        }

        if (options.Overlay?.Menu is not null)
        {
            var menu = options.Overlay.Menu;
            s.Visuals.Menu.X = menu.X ?? s.Visuals.Menu.X;
            s.Visuals.Menu.Y = menu.Y ?? s.Visuals.Menu.Y;
            s.Visuals.Menu.BackgroundColor = menu.BackgroundColor ?? s.Visuals.Menu.BackgroundColor;
            s.Visuals.Menu.TextColor = menu.TextColor ?? s.Visuals.Menu.TextColor;
            s.Visuals.Menu.FontSize = menu.FontSize ?? s.Visuals.Menu.FontSize;
        }

        if (options.Overlay?.InjectionPrompt is not null)
        {
            var prompt = options.Overlay.InjectionPrompt;
            s.Visuals.SystemMessages.Color = prompt.Color ?? s.Visuals.SystemMessages.Color;
            s.Visuals.SystemMessages.FontSize = prompt.FontSize ?? s.Visuals.SystemMessages.FontSize;
        }

        return profile;
    }

    private static GlobalKeybinds MapKeybinds(LegacyToolkitOptions options) => new()
    {
        InjectKey = options.InjectKey ?? "F9",
        MenuToggleKey = options.MenuToggleKey ?? "Insert",
        PanicKey = options.PanicKey ?? "F10",
        SaveSettingsKey = options.SaveSettingsKey ?? "F11",
        RcsToggleKey = options.Rcs?.ToggleKey ?? "F8",
        TbToggleKey = options.Tb?.ToggleKey ?? "F7",
        EnemyEspToggleKey = options.EnemyEsp?.ToggleKey ?? "F6",
        SoundEspToggleKey = options.SoundEsp?.ToggleKey ?? "F5",
        AimHelperToggleKey = options.AimHelper?.ToggleKey ?? "F4",
        AimHelperActivationKey = options.AimHelper?.ActivationKey ?? "",
        TbAutoStrafeKey = options.Tb?.AutoStrafeKey ?? "Space"
    };

    private sealed class LegacyToolkitOptions
    {
        public string? InjectKey { get; set; }
        public string? MenuToggleKey { get; set; }
        public string? PanicKey { get; set; }
        public string? SaveSettingsKey { get; set; }
        public LegacyRcsOptions? Rcs { get; set; }
        public LegacyTbOptions? Tb { get; set; }
        public LegacyEnemyEspOptions? EnemyEsp { get; set; }
        public LegacySoundEspOptions? SoundEsp { get; set; }
        public LegacyAimHelperOptions? AimHelper { get; set; }
        public LegacyEnemyNoiseOptions? EnemyNoise { get; set; }
        public LegacyOverlayOptions? Overlay { get; set; }
    }

    private sealed class LegacyRcsOptions
    {
        public string? ToggleKey { get; set; }
        public bool? Enabled { get; set; }
        public float? Sensitivity { get; set; }
        public float? PitchScale { get; set; }
        public float? YawScale { get; set; }
        public float? FirstBulletCompensateChance { get; set; }
        public float? SubsequentBulletSkipChance { get; set; }
    }

    private sealed class LegacyTbOptions
    {
        public string? ToggleKey { get; set; }
        public string? AutoStrafeKey { get; set; }
        public bool? Enabled { get; set; }
        public bool? AutoStopEnabled { get; set; }
        public float? PreFireFovDegrees { get; set; }
        public int? MinReactionDelayMs { get; set; }
        public int? MaxReactionDelayMs { get; set; }
    }

    private sealed class LegacyEnemyEspOptions
    {
        public string? ToggleKey { get; set; }
        public string? Mode { get; set; }
    }

    private sealed class LegacySoundEspOptions
    {
        public string? ToggleKey { get; set; }
        public bool? Enabled { get; set; }
    }

    private sealed class LegacyAimHelperOptions
    {
        public string? ToggleKey { get; set; }
        public string? ActivationKey { get; set; }
        public bool? Enabled { get; set; }
        public string? PreferredBone { get; set; }
        public float? FovDegrees { get; set; }
    }

    private sealed class LegacyEnemyNoiseOptions
    {
        public float? MaxDistanceUnits { get; set; }
        public string? WaveColor { get; set; }
        public float? WaveLineWidth { get; set; }
        public int? WaveDurationMs { get; set; }
        public float? MinWorldRadius { get; set; }
        public float? MaxWorldRadius { get; set; }
        public int? RingCount { get; set; }
        public float? RingSpacing { get; set; }
    }

    private sealed class LegacyOverlayOptions
    {
        public LegacySkeletonOverlayOptions? EnemyLastSeen { get; set; }
        public LegacyGrenadeOverlayOptions? GrenadeTrajectory { get; set; }
        public LegacyTextPanelOverlayOptions? TeammateStats { get; set; }
        public LegacyTextPanelOverlayOptions? BombStatus { get; set; }
        public LegacyTextPanelOverlayOptions? Clairvoyance { get; set; }
        public LegacyMenuOverlayOptions? Menu { get; set; }
        public LegacyTextPanelOverlayOptions? InjectionPrompt { get; set; }
    }

    private sealed class LegacyMenuOverlayOptions
    {
        public int? X { get; set; }
        public int? Y { get; set; }
        public string? BackgroundColor { get; set; }
        public string? TextColor { get; set; }
        public float? FontSize { get; set; }
    }

    private sealed class LegacyTextPanelOverlayOptions
    {
        public bool? Enabled { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string? Color { get; set; }
        public float? FontSize { get; set; }
    }

    private sealed class LegacySkeletonOverlayOptions
    {
        public string? Color { get; set; }
        public float? LineWidth { get; set; }
    }

    private sealed class LegacyGrenadeOverlayOptions
    {
        public bool? Enabled { get; set; }
        public string? ArcColor { get; set; }
        public string? LandingColor { get; set; }
        public float? ArcLineWidth { get; set; }
        public float? LandingLineWidth { get; set; }
    }
}
