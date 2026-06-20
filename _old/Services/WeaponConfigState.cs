using Cs2Toolkit.Configuration;

namespace Cs2Toolkit.Services;

public sealed class WeaponConfigState
{
    private readonly object _lock = new();
    private ushort _weaponId;
    private TriggerbotLayerSettings _triggerbot = new();
    private RcsLayerSettings _rcs = new();
    private AimHelperLayerSettings _aimHelper = new();

    public ushort WeaponId
    {
        get
        {
            lock (_lock)
                return _weaponId;
        }
    }

    public TriggerbotLayerSettings Triggerbot
    {
        get
        {
            lock (_lock)
                return _triggerbot;
        }
    }

    public RcsLayerSettings Rcs
    {
        get
        {
            lock (_lock)
                return _rcs;
        }
    }

    public AimHelperLayerSettings AimHelper
    {
        get
        {
            lock (_lock)
                return _aimHelper;
        }
    }

    public void Update(ushort weaponId, RuntimeConfigProvider runtimeConfig)
    {
        lock (_lock)
        {
            _weaponId = weaponId;
            _triggerbot = runtimeConfig.ResolveTriggerbot(weaponId);
            _rcs = runtimeConfig.ResolveRcs(weaponId);
            _aimHelper = runtimeConfig.ResolveAimHelper(weaponId);
        }
    }
}
