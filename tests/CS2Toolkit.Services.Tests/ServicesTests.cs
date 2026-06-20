using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services;
using CS2Toolkit.Services.Abstractions;
using Moq;

namespace CS2Toolkit.Services.Tests;

public sealed class FeatureRuntimeStateTests
{
    private readonly FeatureRuntimeState _state = new();

    [Fact]
    public void ApplyFromProfile_hydrates_toggle_fields()
    {
        var profile = new ProfileSettings
        {
            Triggerbot = { Global = { Enabled = true, AutoStopEnabled = true } },
            Rcs = { Global = { Enabled = true } },
            AimHelper = { Global = { Enabled = false } },
            SoundEsp = { Enabled = true },
            EnemyEsp = { Mode = nameof(EnemyEspMode.Full) }
        };

        _state.ApplyFromProfile(profile);

        Assert.True(_state.IsEnabled(FeatureIds.Triggerbot));
        Assert.True(_state.IsEnabled(FeatureIds.Rcs));
        Assert.False(_state.IsEnabled(FeatureIds.AimHelper));
        Assert.True(_state.IsEnabled(FeatureIds.SoundEsp));
        Assert.Equal(EnemyEspMode.Full, _state.EnemyEspMode);
        Assert.True(_state.TriggerbotAutoStopEnabled);
    }

    [Fact]
    public void Toggle_cycles_enemy_esp_modes()
    {
        Assert.Equal(EnemyEspMode.Disabled, _state.EnemyEspMode);

        _state.Toggle(FeatureIds.EnemyEsp);
        Assert.Equal(EnemyEspMode.LastSeen, _state.EnemyEspMode);

        _state.Toggle(FeatureIds.EnemyEsp);
        Assert.Equal(EnemyEspMode.Full, _state.EnemyEspMode);

        _state.Toggle(FeatureIds.EnemyEsp);
        Assert.Equal(EnemyEspMode.Disabled, _state.EnemyEspMode);
    }

    [Fact]
    public void DisableAllCombatFeatures_clears_runtime_state()
    {
        _state.SetEnabled(FeatureIds.Triggerbot, true);
        _state.SetEnabled(FeatureIds.EnemyEsp, true);
        _state.ToggleTriggerbotAutoStop();
        _state.AimHelperActivationHeld = true;

        _state.DisableAllCombatFeatures();

        Assert.False(_state.IsEnabled(FeatureIds.Triggerbot));
        Assert.Equal(EnemyEspMode.Disabled, _state.EnemyEspMode);
        Assert.False(_state.TriggerbotAutoStopEnabled);
        Assert.False(_state.AimHelperActivationHeld);
    }
}

public sealed class ProfileSettingsSaverTests
{
    [Fact]
    public void SaveActiveProfile_writes_runtime_toggles_to_store()
    {
        var state = new FeatureRuntimeState();
        state.SetEnabled(FeatureIds.Triggerbot, true);
        state.SetEnabled(FeatureIds.Rcs, true);
        state.CycleEnemyEspMode();
        state.ToggleTriggerbotAutoStop();

        var store = new Mock<IConfigurationStore>();
        var profile = new ConfigProfile { Name = "Live" };
        store.Setup(s => s.GetActiveProfile()).Returns(profile);
        store.Setup(s => s.UpdateProfile(It.IsAny<ConfigProfile>()))
            .Returns<ConfigProfile>(p => p);

        var saver = new ProfileSettingsSaver(store.Object, state);
        var saved = saver.SaveActiveProfile();

        Assert.True(saved.Settings.Triggerbot.Global.Enabled);
        Assert.True(saved.Settings.Rcs.Global.Enabled);
        Assert.Equal(nameof(EnemyEspMode.LastSeen), saved.Settings.EnemyEsp.Mode);
        Assert.True(saved.Settings.Triggerbot.Global.AutoStopEnabled);
        store.Verify(s => s.UpdateProfile(profile), Times.Once);
    }
}

public sealed class StatusToastStoreTests
{
    [Fact]
    public void Publish_and_persistent_toasts_are_active()
    {
        var store = new StatusToastStore();

        store.Publish("Timed", TimeSpan.FromMinutes(1));
        store.SetPersistent("Attach prompt");

        var active = store.GetActive();
        Assert.Equal(2, active.Count);
        Assert.True(store.HasActive);
    }

    [Fact]
    public void Clear_removes_all_toasts()
    {
        var store = new StatusToastStore();
        store.Publish("Timed", TimeSpan.FromMinutes(1));
        store.SetPersistent("Attach prompt");

        store.Clear();

        Assert.False(store.HasActive);
        Assert.Empty(store.GetActive());
    }
}

public sealed class OverlayColorParserTests
{
    [Theory]
    [InlineData("#FF112233", 0xFF112233u)]
    [InlineData("#112233", 0xFF112233u)]
    [InlineData("invalid", 0xFF00FF00u)]
    public void ParseArgb_parses_hex_or_returns_fallback(string input, uint expected) =>
        Assert.Equal(expected, OverlayColorParser.ParseArgb(input, 0xFF00FF00u));
}
