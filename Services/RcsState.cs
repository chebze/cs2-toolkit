namespace Cs2Toolkit.Services;

public sealed class RcsState
{
    private int _enabled;

    public bool IsEnabled => Volatile.Read(ref _enabled) == 1;

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
