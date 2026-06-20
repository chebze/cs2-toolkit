using System.Text.Json;
using System.Text.Json.Nodes;
using CS2Toolkit.Configuration.Abstractions;
using Microsoft.Extensions.Hosting;

namespace CS2Toolkit.Configuration;

public sealed class JsonConfigurationStore : IConfigurationStore, IConfigurationChangeNotifier
{
    private readonly string _storePath;
    private readonly LegacySettingsMigrator _migrator;
    private readonly object _lock = new();
    private ConfigurationStore _store;

    public event Action? ConfigurationChanged;

    public JsonConfigurationStore(IHostEnvironment environment, LegacySettingsMigrator migrator)
    {
        _migrator = migrator;
        var dataDir = Path.Combine(environment.ContentRootPath, "data", "configs");
        Directory.CreateDirectory(dataDir);
        _storePath = Path.Combine(dataDir, "store.json");
        _store = LoadOrMigrate(environment);
    }

    public ConfigurationStore GetStore()
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

    public string ExportProfile(string profileId)
    {
        var profile = GetProfile(profileId)
            ?? throw new InvalidOperationException($"Profile not found: {profileId}");
        return JsonSerializer.Serialize(profile, ConfigurationJson.Options);
    }

    public ConfigProfile ImportProfile(string json, string? nameOverride = null)
    {
        var profile = JsonSerializer.Deserialize<ConfigProfile>(json, ConfigurationJson.Options)
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

    private ConfigurationStore LoadOrMigrate(IHostEnvironment environment)
    {
        if (File.Exists(_storePath))
        {
            var json = File.ReadAllText(_storePath);
            var store = JsonSerializer.Deserialize<ConfigurationStore>(json, ConfigurationJson.Options);
            if (store is not null && store.Profiles.Count > 0)
                return store;
        }

        var migrated = _migrator.MigrateFromLegacyAppSettings(environment);
        SaveStore(migrated);
        return migrated;
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

    private void SaveLocked() => SaveStore(_store);

    private void SaveStore(ConfigurationStore store)
    {
        var temp = _storePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(store, ConfigurationJson.Options));
        File.Move(temp, _storePath, overwrite: true);
        _store = store;
    }

    private void NotifyChanged() => ConfigurationChanged?.Invoke();

    private static ConfigurationStore CloneStore(ConfigurationStore store)
    {
        var json = JsonSerializer.Serialize(store, ConfigurationJson.Options);
        return JsonSerializer.Deserialize<ConfigurationStore>(json, ConfigurationJson.Options) ?? new ConfigurationStore();
    }

    private static ConfigProfile CloneProfile(ConfigProfile profile)
    {
        var json = JsonSerializer.Serialize(profile, ConfigurationJson.Options);
        return JsonSerializer.Deserialize<ConfigProfile>(json, ConfigurationJson.Options) ?? new ConfigProfile();
    }

    private static T Clone<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, ConfigurationJson.Options);
        return JsonSerializer.Deserialize<T>(json, ConfigurationJson.Options)!;
    }
}
