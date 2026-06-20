using Cs2Toolkit.Configuration;

namespace Cs2Toolkit.Services;

public sealed class RcsState
{
    private int _enabled;

    public bool IsEnabled => Volatile.Read(ref _enabled) == 1;

    public void Initialize(RcsOptions options) => InitializeFromConfig(options);

    public void InitializeFromConfig(RcsOptions options) =>
        Volatile.Write(ref _enabled, options.Enabled ? 1 : 0);

    public bool Toggle()
    {
        while (true)
        {
            var current = Volatile.Read(ref _enabled);
            var next = current == 1 ? 0 : 1;
            if (Interlocked.CompareExchange(ref _enabled, next, current) == current)
                return next == 1;
        }
    }
}
