using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Services;

public sealed class EnemyEspState
{
    private int _mode = (int)EnemyEspMode.LastSeen;

    public EnemyEspMode Mode => (EnemyEspMode)Volatile.Read(ref _mode);

    public void Initialize(EnemyEspOptions options) =>
        Volatile.Write(ref _mode, (int)EnemyEspModeParser.Parse(options.Mode));

    public EnemyEspMode CycleMode()
    {
        while (true)
        {
            var current = Volatile.Read(ref _mode);
            var next = current switch
            {
                (int)EnemyEspMode.Disabled => (int)EnemyEspMode.LastSeen,
                (int)EnemyEspMode.LastSeen => (int)EnemyEspMode.Full,
                _ => (int)EnemyEspMode.Disabled
            };

            if (Interlocked.CompareExchange(ref _mode, next, current) == current)
                return (EnemyEspMode)next;
        }
    }
}
