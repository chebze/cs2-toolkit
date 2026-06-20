using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CS2Toolkit.API.Abstractions;
using CS2Toolkit.API.Endpoints;
using CS2Toolkit.API.Json;
using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CS2Toolkit.API.Tests;

public sealed class ApiTestHost : IAsyncDisposable
{
    private WebApplication? _app;

    public HttpClient Client { get; private set; } = null!;

    public static async Task<ApiTestHost> StartAsync(Action<IServiceCollection> configureServices)
    {
        var host = new ApiTestHost();
        WebApplication? app = null;
        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Testing",
                Args = ["--urls", "http://127.0.0.1:0"]
            });

            configureServices(builder.Services);
            app = builder.Build();
            app.MapToolkitApi();
            await app.StartAsync();
            host._app = app;
            host.Client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
            return host;
        }
        catch
        {
            if (app is not null)
                await app.DisposeAsync();

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        if (_app is not null)
            await _app.StopAsync();
        if (_app is not null)
            await _app.DisposeAsync();
    }
}

public sealed class WeaponsApiTests
{
    [Fact]
    public async Task GetWeapons_returns_catalog()
    {
        await using var host = await ApiTestHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IConfigurationStore>());
            services.AddSingleton(Mock.Of<IActiveProfileSwitcher>());
            services.AddSingleton(Mock.Of<IDashboardInfoProvider>());
            services.AddSingleton(Mock.Of<IRadarStreamSource>());
        });

        var response = await host.Client.GetAsync("/api/weapons");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var weapons = await response.Content.ReadFromJsonAsync<List<WeaponDefinitionDto>>();
        Assert.NotNull(weapons);
        Assert.Contains(weapons, w => w.Id == 7 && w.Name == "AK-47");
    }

    private sealed record WeaponDefinitionDto(ushort Id, string Name, string Category);
}

public sealed class ConfigsApiTests
{
    [Fact]
    public async Task PutActiveProfile_applies_toggles_only_when_toggle_fields_change()
    {
        var profile = CreateProfile("profile-1", "Default");
        var store = CreateStore(profile);
        var switcher = new Mock<IActiveProfileSwitcher>();

        await using var host = await ApiTestHost.StartAsync(services =>
        {
            services.AddSingleton(store.Object);
            services.AddSingleton(switcher.Object);
            services.AddSingleton(Mock.Of<IDashboardInfoProvider>());
            services.AddSingleton(Mock.Of<IRadarStreamSource>());
        });

        profile.Settings.SoundEsp.WaveColor = "#123456";
        var visualOnly = await host.Client.PutAsJsonAsync($"/api/configs/{profile.Id}", profile);
        Assert.Equal(HttpStatusCode.OK, visualOnly.StatusCode);
        switcher.Verify(s => s.ApplyActiveProfileToggles(It.IsAny<ProfileSettings>()), Times.Never);

        var toggleUpdate = CloneProfile(profile);
        toggleUpdate.Settings.Triggerbot.Global.Enabled = true;
        var toggleChange = await host.Client.PutAsJsonAsync($"/api/configs/{profile.Id}", toggleUpdate);
        Assert.Equal(HttpStatusCode.OK, toggleChange.StatusCode);
        switcher.Verify(s => s.ApplyActiveProfileToggles(It.IsAny<ProfileSettings>()), Times.Once);
    }

    [Fact]
    public async Task PutInactiveProfile_skips_runtime_toggle_apply_even_when_toggles_change()
    {
        var active = CreateProfile("profile-active", "Active");
        var inactive = CreateProfile("profile-inactive", "Inactive");
        var store = CreateStore(active, inactive);
        var switcher = new Mock<IActiveProfileSwitcher>();

        await using var host = await ApiTestHost.StartAsync(services =>
        {
            services.AddSingleton(store.Object);
            services.AddSingleton(switcher.Object);
            services.AddSingleton(Mock.Of<IDashboardInfoProvider>());
            services.AddSingleton(Mock.Of<IRadarStreamSource>());
        });

        var toggleUpdate = CloneProfile(inactive);
        toggleUpdate.Settings.Triggerbot.Global.Enabled = true;

        var response = await host.Client.PutAsJsonAsync($"/api/configs/{inactive.Id}", toggleUpdate);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        switcher.Verify(s => s.ApplyActiveProfileToggles(It.IsAny<ProfileSettings>()), Times.Never);
    }

    [Fact]
    public async Task ActivateProfile_calls_switcher()
    {
        var profile = CreateProfile("profile-1", "Default");
        var store = CreateStore(profile);
        var switcher = new Mock<IActiveProfileSwitcher>();

        await using var host = await ApiTestHost.StartAsync(services =>
        {
            services.AddSingleton(store.Object);
            services.AddSingleton(switcher.Object);
            services.AddSingleton(Mock.Of<IDashboardInfoProvider>());
            services.AddSingleton(Mock.Of<IRadarStreamSource>());
        });

        var response = await host.Client.PostAsync($"/api/configs/{profile.Id}/activate", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        switcher.Verify(s => s.SwitchTo(profile.Id), Times.Once);
    }

    [Fact]
    public async Task GetConfigs_returns_store_snapshot()
    {
        var profile = CreateProfile("profile-1", "Default");
        var store = CreateStore(profile);

        await using var host = await ApiTestHost.StartAsync(services =>
        {
            services.AddSingleton(store.Object);
            services.AddSingleton(Mock.Of<IActiveProfileSwitcher>());
            services.AddSingleton(Mock.Of<IDashboardInfoProvider>());
            services.AddSingleton(Mock.Of<IRadarStreamSource>());
        });

        var response = await host.Client.GetAsync("/api/configs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("profiles", out var profiles));
        Assert.Equal(1, profiles.GetArrayLength());
    }

    private static ConfigProfile CreateProfile(string id, string name) =>
        new() { Id = id, Name = name };

    private static Mock<IConfigurationStore> CreateStore(ConfigProfile activeProfile, params ConfigProfile[] profiles)
    {
        var store = new Mock<IConfigurationStore>();
        var persistedProfiles = profiles.Select(CloneProfile).ToDictionary(p => p.Id);
        if (!persistedProfiles.ContainsKey(activeProfile.Id))
            persistedProfiles[activeProfile.Id] = CloneProfile(activeProfile);

        var configurationStore = new ConfigurationStore
        {
            ActiveProfileId = activeProfile.Id,
            DefaultProfileId = activeProfile.Id,
            Profiles = persistedProfiles.Values.Select(CloneProfile).ToList()
        };

        store.Setup(s => s.GetStore()).Returns(() =>
        {
            configurationStore.Profiles = persistedProfiles.Values.Select(CloneProfile).ToList();
            return configurationStore;
        });
        store.Setup(s => s.GetActiveProfile()).Returns(() => CloneProfile(persistedProfiles[activeProfile.Id]));
        store.Setup(s => s.GetProfile(It.IsAny<string>()))
            .Returns<string>(id => persistedProfiles.TryGetValue(id, out var profile) ? CloneProfile(profile) : null);
        store.Setup(s => s.UpdateProfile(It.IsAny<ConfigProfile>()))
            .Returns<ConfigProfile>(updated =>
            {
                persistedProfiles[updated.Id] = CloneProfile(updated);
                return CloneProfile(updated);
            });

        return store;
    }

    private static Mock<IConfigurationStore> CreateStore(ConfigProfile profile) =>
        CreateStore(profile, profile);

    private static ConfigProfile CloneProfile(ConfigProfile profile)
    {
        var json = JsonSerializer.Serialize(profile, ToolkitJsonSerializerOptions.Web);
        return JsonSerializer.Deserialize<ConfigProfile>(json, ToolkitJsonSerializerOptions.Web)!;
    }
}

public sealed class ProfileRuntimeTogglesTests
{
    [Fact]
    public void Differ_detects_toggle_field_changes_only()
    {
        var before = new ProfileSettings
        {
            Triggerbot = { Global = { Enabled = false } },
            SoundEsp = { Enabled = false, WaveColor = "#111111" }
        };
        var visualOnly = new ProfileSettings
        {
            Triggerbot = { Global = { Enabled = false } },
            SoundEsp = { Enabled = false, WaveColor = "#222222" }
        };
        var toggleChange = new ProfileSettings
        {
            Triggerbot = { Global = { Enabled = true } },
            SoundEsp = { Enabled = false, WaveColor = "#222222" }
        };

        Assert.False(ProfileRuntimeToggles.Differ(before, visualOnly));
        Assert.True(ProfileRuntimeToggles.Differ(before, toggleChange));
    }
}

public sealed class DashboardApiTests
{
    [Fact]
    public async Task GetDashboard_returns_provider_payload()
    {
        var dashboard = new DashboardInfo(
            new ActiveProfileSummary("p1", "Default", "F6"),
            "p1",
            ["http://localhost:8080"],
            8080,
            "/radar");

        await using var host = await ApiTestHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IConfigurationStore>());
            services.AddSingleton(Mock.Of<IActiveProfileSwitcher>());
            services.AddSingleton<IDashboardInfoProvider>(_ => new FixedDashboardInfoProvider(dashboard));
            services.AddSingleton(Mock.Of<IRadarStreamSource>());
        });

        var response = await host.Client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<DashboardInfo>(ToolkitJsonSerializerOptions.Web);
        Assert.NotNull(payload);
        Assert.Equal("Default", payload.ActiveProfile.Name);
        Assert.Equal(8080, payload.WebPort);
    }

    private sealed class FixedDashboardInfoProvider(DashboardInfo info) : IDashboardInfoProvider
    {
        public DashboardInfo GetDashboardInfo() => info;
    }
}

public sealed class RadarApiTests
{
    [Fact]
    public async Task GetRadarSnapshot_returns_mock_payload()
    {
        var radar = new Mock<IRadarStreamSource>();
        radar.Setup(r => r.GetSnapshot()).Returns(RadarSnapshot.Idle);
        radar.Setup(r => r.Version).Returns(1);
        radar.Setup(r => r.GetSnapshotJson()).Returns("{\"version\":1}");

        await using var host = await ApiTestHost.StartAsync(services =>
        {
            services.AddSingleton(Mock.Of<IConfigurationStore>());
            services.AddSingleton(Mock.Of<IActiveProfileSwitcher>());
            services.AddSingleton(Mock.Of<IDashboardInfoProvider>());
            services.AddSingleton(radar.Object);
        });

        var response = await host.Client.GetAsync("/api/radar/snapshot");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
