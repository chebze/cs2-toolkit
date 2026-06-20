using CS2Toolkit.Configuration.Abstractions;
using Microsoft.Extensions.Options;

namespace CS2Toolkit.Configuration;

public sealed class ActiveConfiguration : IActiveConfiguration, IKeybindConfiguration
{
    private readonly IConfigurationStore _store;
    private readonly ISettingsResolver _resolver;
    private readonly IOptionsMonitor<ToolkitHostSettings> _hostSettings;
    private readonly object _lock = new();
    private ToolkitSettings _current;

    public ActiveConfiguration(
        IConfigurationStore store,
        ISettingsResolver resolver,
        IOptionsMonitor<ToolkitHostSettings> hostSettings,
        IConfigurationChangeNotifier changeNotifier)
    {
        _store = store;
        _resolver = resolver;
        _hostSettings = hostSettings;
        _current = BuildSettings();
        changeNotifier.ConfigurationChanged += Refresh;
        _hostSettings.OnChange((_, _) => Refresh());
    }

    public ToolkitSettings Current
    {
        get
        {
            lock (_lock)
                return _current;
        }
    }

    public GlobalKeybinds Keybinds => Current.Keybinds;

    public ResolvedWeaponSettings ResolveWeapon(ushort weaponId) =>
        _resolver.ResolveWeaponSettings(Current.Profile, weaponId);

    public void Refresh()
    {
        lock (_lock)
            _current = BuildSettings();
    }

    private ToolkitSettings BuildSettings()
    {
        var store = _store.GetStore();
        var profile = _store.GetActiveProfile();

        return new ToolkitSettings
        {
            Keybinds = store.Keybinds,
            Host = _hostSettings.CurrentValue,
            Profile = profile.Settings,
            WebPort = store.WebPort,
            ActiveProfileId = profile.Id,
            ActiveProfileName = profile.Name
        };
    }
}
