using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Cs2Toolkit.Configuration;
using Microsoft.Extensions.Hosting;

namespace Cs2Toolkit.Configuration;

public sealed class ConfigManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _storePath;
    private readonly object _lock = new();
    private ConfigStore _store;

    public event Action? StoreChanged;

    public ConfigManager(IHostEnvironment environment)
    {
        var dataDir = Path.Combine(environment.ContentRootPath, "data", "configs");
        Directory.CreateDirectory(dataDir);
        _storePath = Path.Combine(dataDir, "store.json");
        _store = LoadOrMigrate(environment);
    }

    public ConfigStore GetStore()
    {
        lock (_lock)
            return CloneStore(_store);
    }

    public ConfigProfile GetActiveProfile()
    {
        lock (_lock)
            return CloneProfile(ResolveActiveProfile());
    }

    public ConfigProfile? GetProfile(string id)
    {
        lock (_lock)
        {
            var profile = _store.Profiles.FirstOrDefault(p => p.Id == id);
            return profile is null ? null : CloneProfile(profile);
        }
    }

    public void SetActiveProfile(string profileId)
    {
        lock (_lock)
        {
            if (_store.Profiles.All(p => p.Id != profileId))
                throw new InvalidOperationException($"Profile not found: {profileId}");

            _store.ActiveProfileId = profileId;
            SaveLocked();
        }

        NotifyChanged();
    }

    public void SetDefaultProfile(string profileId)
    {
        lock (_lock)
        {
            if (_store.Profiles.All(p => p.Id != profileId))
                throw new InvalidOperationException($"Profile not found: {profileId}");

            _store.DefaultProfileId = profileId;
            SaveLocked();
        }

        NotifyChanged();
    }

    public ConfigProfile CreateProfile(string name)
    {
        var profile = new ConfigProfile { Name = name };
        lock (_lock)
        {
            _store.Profiles.Add(profile);
            if (string.IsNullOrEmpty(_store.DefaultProfileId))
            {
                _store.DefaultProfileId = profile.Id;
                _store.ActiveProfileId = profile.Id;
            }

            SaveLocked();
        }

        NotifyChanged();
        return CloneProfile(profile);
    }

    public ConfigProfile UpdateProfile(ConfigProfile profile)
    {
        lock (_lock)
        {
            var index = _store.Profiles.FindIndex(p => p.Id == profile.Id);
            if (index < 0)
                throw new InvalidOperationException($"Profile not found: {profile.Id}");

            _store.Profiles[index] = CloneProfile(profile);
            SaveLocked();
        }

        NotifyChanged();
        return GetProfile(profile.Id)!;
    }

    public void DeleteProfile(string profileId)
    {
        lock (_lock)
        {
            if (_store.Profiles.Count <= 1)
                throw new InvalidOperationException("Cannot delete the last profile.");

            _store.Profiles.RemoveAll(p => p.Id == profileId);

            if (_store.DefaultProfileId == profileId)
                _store.DefaultProfileId = _store.Profiles[0].Id;

            if (_store.ActiveProfileId == profileId)
                _store.ActiveProfileId = _store.DefaultProfileId;

            SaveLocked();
        }

        NotifyChanged();
    }

    public void UpdateKeybinds(GlobalKeybinds keybinds)
    {
        lock (_lock)
        {
            _store.Keybinds = Clone(keybinds);
            SaveLocked();
        }

        NotifyChanged();
    }

    public void UpdateWebPort(int port)
    {
        lock (_lock)
        {
            _store.WebPort = port;
            SaveLocked();
        }

        NotifyChanged();
    }

    public void UpdatePublicTunnelSettings(
        bool enabled,
        string server,
        string? subdomain,
        int maxConnections)
    {
        lock (_lock)
        {
            _store.PublicTunnelEnabled = enabled;
            _store.PublicTunnelServer = string.IsNullOrWhiteSpace(server)
                ? "https://localtunnel.me"
                : server;
            _store.PublicTunnelSubdomain = string.IsNullOrWhiteSpace(subdomain) ? null : subdomain;
            _store.PublicTunnelMaxConnections = Math.Clamp(maxConnections, 1, 10);
            SaveLocked();
        }

        NotifyChanged();
    }

    public string ExportProfile(string profileId)
    {
        var profile = GetProfile(profileId)
            ?? throw new InvalidOperationException($"Profile not found: {profileId}");
        return JsonSerializer.Serialize(profile, JsonOptions);
    }

    public ConfigProfile ImportProfile(string json, string? nameOverride = null)
    {
        var profile = JsonSerializer.Deserialize<ConfigProfile>(json, JsonOptions)
            ?? throw new InvalidOperationException("Invalid profile JSON.");

        profile.Id = Guid.NewGuid().ToString("N");
        if (!string.IsNullOrWhiteSpace(nameOverride))
            profile.Name = nameOverride;

        lock (_lock)
        {
            _store.Profiles.Add(CloneProfile(profile));
            SaveLocked();
        }

        NotifyChanged();
        return GetProfile(profile.Id)!;
    }

    public ToolkitOptions BuildToolkitOptions()
    {
        lock (_lock)
        {
            var profile = ResolveActiveProfile();
            return MapToToolkitOptions(_store, profile);
        }
    }

    private ConfigProfile ResolveActiveProfile()
    {
        var activeId = _store.ActiveProfileId ?? _store.DefaultProfileId;
        var profile = _store.Profiles.FirstOrDefault(p => p.Id == activeId)
            ?? _store.Profiles.FirstOrDefault()
            ?? new ConfigProfile();

        if (string.IsNullOrEmpty(_store.DefaultProfileId))
            _store.DefaultProfileId = profile.Id;

        if (string.IsNullOrEmpty(_store.ActiveProfileId))
            _store.ActiveProfileId = profile.Id;

        return profile;
    }

    private ConfigStore LoadOrMigrate(IHostEnvironment environment)
    {
        if (File.Exists(_storePath))
        {
            var json = File.ReadAllText(_storePath);
            var store = JsonSerializer.Deserialize<ConfigStore>(json, JsonOptions);
            if (store is not null && store.Profiles.Count > 0)
                return store;
        }

        var migrated = MigrateFromAppSettings(environment);
        SaveStore(migrated);
        return migrated;
    }

    private static ConfigStore MigrateFromAppSettings(IHostEnvironment environment)
    {
        var appSettingsPath = Path.Combine(environment.ContentRootPath, "appsettings.json");
        ToolkitOptions? legacy = null;

        if (File.Exists(appSettingsPath))
        {
            var root = JsonNode.Parse(File.ReadAllText(appSettingsPath)) as JsonObject;
            if (root?[ToolkitOptions.SectionName] is JsonNode toolkitNode)
            {
                legacy = toolkitNode.Deserialize<ToolkitOptions>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }

        legacy ??= new ToolkitOptions();
        var profile = MapFromToolkitOptions(legacy, "Default");
        return new ConfigStore
        {
            DefaultProfileId = profile.Id,
            ActiveProfileId = profile.Id,
            Keybinds = MapKeybinds(legacy),
            Profiles = [profile],
            WebPort = 8080
        };
    }

    private void SaveLocked() => SaveStore(_store);

    private void SaveStore(ConfigStore store)
    {
        var temp = _storePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(store, JsonOptions));
        File.Move(temp, _storePath, overwrite: true);
        _store = store;
    }

    private void NotifyChanged() => StoreChanged?.Invoke();

    private static ConfigStore CloneStore(ConfigStore store)
    {
        var json = JsonSerializer.Serialize(store, JsonOptions);
        return JsonSerializer.Deserialize<ConfigStore>(json, JsonOptions) ?? new ConfigStore();
    }

    private static ConfigProfile CloneProfile(ConfigProfile profile)
    {
        var json = JsonSerializer.Serialize(profile, JsonOptions);
        return JsonSerializer.Deserialize<ConfigProfile>(json, JsonOptions) ?? new ConfigProfile();
    }

    private static T Clone<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    public static ConfigProfile MapFromToolkitOptions(ToolkitOptions options, string name)
    {
        var profile = new ConfigProfile { Name = name };
        var s = profile.Settings;

        s.Triggerbot.Global = new TriggerbotLayerSettings
        {
            Enabled = options.Tb.Enabled,
            AutoStopEnabled = options.Tb.AutoStopEnabled,
            PreFireFovDegrees = options.Tb.PreFireFovDegrees,
            MinReactionDelayMs = options.Tb.MinReactionDelayMs,
            MaxReactionDelayMs = options.Tb.MaxReactionDelayMs
        };

        s.Rcs.Global = new RcsLayerSettings
        {
            Enabled = options.Rcs.Enabled,
            Sensitivity = options.Rcs.Sensitivity,
            PitchScale = options.Rcs.PitchScale,
            YawScale = options.Rcs.YawScale,
            FirstBulletCompensateChance = options.Rcs.FirstBulletCompensateChance,
            SubsequentBulletSkipChance = options.Rcs.SubsequentBulletSkipChance
        };

        s.AimHelper.Global = new AimHelperLayerSettings
        {
            Enabled = options.AimHelper.Enabled,
            PreferredBone = options.AimHelper.PreferredBone,
            FovDegrees = options.AimHelper.FovDegrees
        };

        s.EnemyEsp.Mode = options.EnemyEsp.Mode;
        s.EnemyEsp.SkeletonColor = options.Overlay.EnemyLastSeen.Color;
        s.EnemyEsp.SkeletonLineWidth = options.Overlay.EnemyLastSeen.LineWidth;

        s.SoundEsp.Enabled = options.SoundEsp.Enabled;
        s.SoundEsp.WaveColor = options.EnemyNoise.WaveColor;
        s.SoundEsp.WaveLineWidth = options.EnemyNoise.WaveLineWidth;
        s.SoundEsp.WaveDurationMs = options.EnemyNoise.WaveDurationMs;
        s.SoundEsp.MinWorldRadius = options.EnemyNoise.MinWorldRadius;
        s.SoundEsp.MaxWorldRadius = options.EnemyNoise.MaxWorldRadius;
        s.SoundEsp.RingCount = options.EnemyNoise.RingCount;
        s.SoundEsp.RingSpacing = options.EnemyNoise.RingSpacing;

        s.Visuals.Grenade.Enabled = options.Overlay.GrenadeTrajectory.Enabled;
        s.Visuals.Grenade.ArcColor = options.Overlay.GrenadeTrajectory.ArcColor;
        s.Visuals.Grenade.LandingColor = options.Overlay.GrenadeTrajectory.LandingColor;
        s.Visuals.Grenade.ArcLineWidth = options.Overlay.GrenadeTrajectory.ArcLineWidth;
        s.Visuals.Grenade.LandingLineWidth = options.Overlay.GrenadeTrajectory.LandingLineWidth;

        return profile;
    }

    public static GlobalKeybinds MapKeybinds(ToolkitOptions options) => new()
    {
        InjectKey = options.InjectKey,
        MenuToggleKey = options.MenuToggleKey,
        PanicKey = options.PanicKey,
        SaveSettingsKey = options.SaveSettingsKey,
        RcsToggleKey = options.Rcs.ToggleKey,
        TbToggleKey = options.Tb.ToggleKey,
        EnemyEspToggleKey = options.EnemyEsp.ToggleKey,
        SoundEspToggleKey = options.SoundEsp.ToggleKey,
        AimHelperToggleKey = options.AimHelper.ToggleKey,
        AimHelperActivationKey = options.AimHelper.ActivationKey,
        TbAutoStrafeKey = options.Tb.AutoStrafeKey
    };

    public static ToolkitOptions MapToToolkitOptions(ConfigStore store, ConfigProfile profile)
    {
        var s = profile.Settings;
        var keybinds = store.Keybinds;
        var tb = s.Triggerbot.Global;
        var rcs = s.Rcs.Global;
        var aim = s.AimHelper.Global;

        return new ToolkitOptions
        {
            InjectKey = keybinds.InjectKey,
            MenuToggleKey = keybinds.MenuToggleKey,
            PanicKey = keybinds.PanicKey,
            SaveSettingsKey = keybinds.SaveSettingsKey,
            Rcs = new RcsOptions
            {
                ToggleKey = keybinds.RcsToggleKey,
                Enabled = rcs.Enabled ?? false,
                Sensitivity = rcs.Sensitivity ?? 1.25f,
                PitchScale = rcs.PitchScale ?? 2f,
                YawScale = rcs.YawScale ?? 2f,
                FirstBulletCompensateChance = rcs.FirstBulletCompensateChance ?? 0.5f,
                SubsequentBulletSkipChance = rcs.SubsequentBulletSkipChance ?? 0.2f
            },
            Tb = new TbOptions
            {
                ToggleKey = keybinds.TbToggleKey,
                AutoStrafeKey = keybinds.TbAutoStrafeKey,
                Enabled = tb.Enabled ?? false,
                AutoStopEnabled = tb.AutoStopEnabled ?? false,
                PreFireFovDegrees = tb.PreFireFovDegrees ?? 0.7f,
                MinReactionDelayMs = tb.MinReactionDelayMs ?? 200,
                MaxReactionDelayMs = tb.MaxReactionDelayMs ?? 400
            },
            EnemyEsp = new EnemyEspOptions
            {
                ToggleKey = keybinds.EnemyEspToggleKey,
                Mode = s.EnemyEsp.Mode
            },
            SoundEsp = new SoundEspOptions
            {
                ToggleKey = keybinds.SoundEspToggleKey,
                Enabled = s.SoundEsp.Enabled
            },
            AimHelper = new AimHelperOptions
            {
                ToggleKey = keybinds.AimHelperToggleKey,
                ActivationKey = keybinds.AimHelperActivationKey,
                Enabled = aim.Enabled ?? false,
                PreferredBone = aim.PreferredBone ?? "Head",
                FovDegrees = aim.FovDegrees ?? 3f
            },
            EnemyNoise = new EnemyNoiseOptions
            {
                WaveColor = s.SoundEsp.WaveColor,
                WaveLineWidth = s.SoundEsp.WaveLineWidth,
                WaveDurationMs = s.SoundEsp.WaveDurationMs,
                MinWorldRadius = s.SoundEsp.MinWorldRadius,
                MaxWorldRadius = s.SoundEsp.MaxWorldRadius,
                RingCount = s.SoundEsp.RingCount,
                RingSpacing = s.SoundEsp.RingSpacing
            },
            Overlay = new OverlayOptions
            {
                EnemyLastSeen = new SkeletonOverlayOptions
                {
                    Color = s.EnemyEsp.SkeletonColor,
                    LineWidth = s.EnemyEsp.SkeletonLineWidth
                },
                GrenadeTrajectory = new GrenadeOverlayOptions
                {
                    Enabled = s.Visuals.Grenade.Enabled,
                    ArcColor = s.Visuals.Grenade.ArcColor,
                    LandingColor = s.Visuals.Grenade.LandingColor,
                    ArcLineWidth = s.Visuals.Grenade.ArcLineWidth,
                    LandingLineWidth = s.Visuals.Grenade.LandingLineWidth
                }
            }
        };
    }
}
