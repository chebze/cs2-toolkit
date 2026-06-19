using Cs2Toolkit.Configuration;
using Cs2Toolkit.Utilities;
using System.Windows.Forms;

namespace Cs2Toolkit.Services;

public sealed class GlobalKeybindState
{
    private readonly RuntimeConfigProvider _runtimeConfig;
    private readonly object _lock = new();
    private GlobalKeybinds _keybinds = new();

    public GlobalKeybindState(RuntimeConfigProvider runtimeConfig)
    {
        _runtimeConfig = runtimeConfig;
        Refresh();
        _runtimeConfig.ConfigChanged += Refresh;
    }

    public GlobalKeybinds Current
    {
        get
        {
            lock (_lock)
                return _keybinds;
        }
    }

    public Keys ParseToggleKey(Func<GlobalKeybinds, string> selector) =>
        KeyParser.Parse(selector(Current));

    private void Refresh()
    {
        lock (_lock)
            _keybinds = _runtimeConfig.Current is null
                ? new GlobalKeybinds()
                : ConfigManager.MapKeybinds(_runtimeConfig.Current);
    }
}
