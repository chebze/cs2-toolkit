using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Input.Abstractions;

namespace CS2Toolkit.Input;

public sealed class KeybindMatcher : IKeybindMatcher
{
    private readonly IKeybindConfiguration _keybindConfiguration;
    private readonly IConfigurationChangeNotifier _changeNotifier;
    private readonly object _lock = new();
    private IReadOnlyList<KeybindDefinition> _keybinds;
    private Dictionary<int, KeybindDefinition> _byVirtualKey = new();

    public KeybindMatcher(
        IKeybindConfiguration keybindConfiguration,
        IConfigurationChangeNotifier changeNotifier)
    {
        _keybindConfiguration = keybindConfiguration;
        _changeNotifier = changeNotifier;
        _keybinds = BuildKeybinds(_keybindConfiguration.Keybinds);
        _byVirtualKey = BuildLookup(_keybinds);
        _changeNotifier.ConfigurationChanged += OnConfigurationChanged;
    }

    public KeyCode ParseKey(string keyName) => KeyParser.Parse(keyName);

    public IReadOnlyList<KeybindDefinition> GetKeybinds()
    {
        lock (_lock)
            return _keybinds;
    }

    public bool TryMatchKeyDown(KeyInputEvent input, out KeybindMatch match)
    {
        lock (_lock)
        {
            if (_byVirtualKey.TryGetValue(input.Key.VirtualKey, out var definition))
            {
                match = new KeybindMatch(definition.ActionId, definition.KeyName);
                return true;
            }
        }

        match = default!;
        return false;
    }

    private void OnConfigurationChanged()
    {
        lock (_lock)
        {
            _keybinds = BuildKeybinds(_keybindConfiguration.Keybinds);
            _byVirtualKey = BuildLookup(_keybinds);
        }
    }

    private static IReadOnlyList<KeybindDefinition> BuildKeybinds(GlobalKeybinds keybinds) =>
    [
        new(ToolkitKeybindActions.Inject, keybinds.InjectKey),
        new(ToolkitKeybindActions.MenuToggle, keybinds.MenuToggleKey),
        new(ToolkitKeybindActions.Panic, keybinds.PanicKey),
        new(ToolkitKeybindActions.SaveSettings, keybinds.SaveSettingsKey),
        new(ToolkitKeybindActions.RcsToggle, keybinds.RcsToggleKey),
        new(ToolkitKeybindActions.TriggerbotToggle, keybinds.TbToggleKey),
        new(ToolkitKeybindActions.EnemyEspToggle, keybinds.EnemyEspToggleKey),
        new(ToolkitKeybindActions.SoundEspToggle, keybinds.SoundEspToggleKey),
        new(ToolkitKeybindActions.BulletTracersToggle, keybinds.BulletTracersToggleKey),
        new(ToolkitKeybindActions.AimHelperToggle, keybinds.AimHelperToggleKey),
        new(ToolkitKeybindActions.AimHelperActivation, keybinds.AimHelperActivationKey),
        new(ToolkitKeybindActions.TriggerbotAutoStrafe, keybinds.TbAutoStrafeKey)
    ];

    private static Dictionary<int, KeybindDefinition> BuildLookup(IEnumerable<KeybindDefinition> keybinds)
    {
        var lookup = new Dictionary<int, KeybindDefinition>();
        foreach (var keybind in keybinds)
        {
            var code = KeyParser.Parse(keybind.KeyName);
            if (code.IsNone || string.IsNullOrWhiteSpace(keybind.KeyName))
                continue;

            lookup[code.VirtualKey] = keybind;
        }

        return lookup;
    }
}
