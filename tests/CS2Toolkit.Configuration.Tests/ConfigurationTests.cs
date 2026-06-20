using CS2Toolkit.Configuration;
using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace CS2Toolkit.Configuration.Tests;

public sealed class SettingsResolverTests
{
    private readonly SettingsResolver _resolver = new();

    [Fact]
    public void ResolveWeaponSettings_merges_global_type_and_weapon_layers()
    {
        const ushort ak47 = 7;
        var profile = new ProfileSettings
        {
            Triggerbot = new LayeredWeaponSettings<TriggerbotLayerSettings>
            {
                Global = new TriggerbotLayerSettings { Enabled = false, PreFireFovDegrees = 0.5f },
                ByWeaponType =
                {
                    ["Rifle"] = new TriggerbotLayerSettings { Enabled = true, PreFireFovDegrees = 0.8f }
                },
                ByWeapon =
                {
                    ["7"] = new TriggerbotLayerSettings { PreFireFovDegrees = 1.2f }
                }
            }
        };

        var resolved = _resolver.ResolveWeaponSettings(profile, ak47).Triggerbot;

        Assert.True(resolved.Enabled);
        Assert.Equal(1.2f, resolved.PreFireFovDegrees);
    }

    [Fact]
    public void ResolveWeaponSettings_unknown_weapon_uses_global_only()
    {
        var profile = new ProfileSettings
        {
            Rcs = new LayeredWeaponSettings<RcsLayerSettings>
            {
                Global = new RcsLayerSettings { Enabled = true, PitchScale = 1.1f }
            }
        };

        var resolved = _resolver.ResolveRcs(profile, 9999);

        Assert.True(resolved.Enabled);
        Assert.Equal(1.1f, resolved.PitchScale);
    }
}

public sealed class WeaponCatalogTests
{
    [Fact]
    public void GetCategory_returns_rifle_for_ak47() =>
        Assert.Equal(WeaponCategory.Rifle, WeaponCatalog.GetCategory(7));

    [Fact]
    public void GetCategory_unknown_weapon_returns_other() =>
        Assert.Equal(WeaponCategory.Other, WeaponCatalog.GetCategory(9999));

    [Fact]
    public void All_contains_unique_weapon_ids() =>
        Assert.Equal(WeaponCatalog.All.Count, WeaponCatalog.All.Select(w => w.Id).Distinct().Count());
}

public sealed class JsonConfigurationStoreTests : IDisposable
{
    private readonly string _root;
    private readonly JsonConfigurationStore _store;

    public JsonConfigurationStoreTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "cs2-toolkit-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        _store = new JsonConfigurationStore(
            new TestHostEnvironment(_root),
            new LegacySettingsMigrator(),
            NullLogger<JsonConfigurationStore>.Instance);
    }

    [Fact]
    public void CreateProfile_persists_and_returns_clone()
    {
        var created = _store.CreateProfile("Competitive");
        var loaded = _store.GetProfile(created.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Competitive", loaded.Name);
        Assert.NotSame(created, loaded);
    }

    [Fact]
    public void SetActiveProfile_switches_active_profile()
    {
        var first = _store.GetActiveProfile();
        var second = _store.CreateProfile("Secondary");

        _store.SetActiveProfile(second.Id);

        Assert.Equal(second.Id, _store.GetActiveProfile().Id);
        Assert.NotEqual(first.Id, _store.GetActiveProfile().Id);
    }

    [Fact]
    public void UpdateProfile_round_trips_settings()
    {
        var profile = _store.GetActiveProfile();
        profile.Settings.SoundEsp.WaveColor = "#AABBCC";

        _store.UpdateProfile(profile);
        var reloaded = _store.GetProfile(profile.Id)!;

        Assert.Equal("#AABBCC", reloaded.Settings.SoundEsp.WaveColor);
    }

    [Fact]
    public void DeleteProfile_cannot_remove_last_profile()
    {
        while (_store.GetStore().Profiles.Count > 1)
            _store.DeleteProfile(_store.GetStore().Profiles[^1].Id);

        var last = _store.GetActiveProfile();
        Assert.Throws<InvalidOperationException>(() => _store.DeleteProfile(last.Id));
    }

    [Fact]
    public void ImportProfile_assigns_new_id_and_optional_name()
    {
        var source = _store.GetActiveProfile();
        var json = _store.ExportProfile(source.Id);

        var imported = _store.ImportProfile(json, "Imported");

        Assert.NotEqual(source.Id, imported.Id);
        Assert.Equal("Imported", imported.Name);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // Best-effort cleanup for temp test directories.
        }
    }

    private sealed class TestHostEnvironment(string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "CS2Toolkit.Tests";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
