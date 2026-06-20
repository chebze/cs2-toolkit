using Cs2Toolkit.Configuration;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Services;

public sealed class RuntimeConfigProvider
{
    private readonly ConfigManager _configManager;
    private readonly object _lock = new();
    private ToolkitOptions _current;
    private ConfigProfile _activeProfile;
    private ProfileSettings _activeSettings;

    public event Action? ConfigChanged;

    public RuntimeConfigProvider(ConfigManager configManager)
    {
        _configManager = configManager;
        _activeProfile = configManager.GetActiveProfile();
        _activeSettings = _activeProfile.Settings;
        _current = configManager.BuildToolkitOptions();
        _configManager.StoreChanged += ReloadFromStore;
    }

    public ToolkitOptions Current
    {
        get
        {
            lock (_lock)
                return _current;
        }
    }

    public ConfigProfile ActiveProfile
    {
        get
        {
            lock (_lock)
                return _activeProfile;
        }
    }

    public ProfileSettings ActiveSettings
    {
        get
        {
            lock (_lock)
                return _activeSettings;
        }
    }

    public TriggerbotLayerSettings ResolveTriggerbot(ushort weaponId) =>
        WeaponSettingsResolver.Resolve(ActiveSettings.Triggerbot, weaponId);

    public RcsLayerSettings ResolveRcs(ushort weaponId) =>
        WeaponSettingsResolver.Resolve(ActiveSettings.Rcs, weaponId);

    public AimHelperLayerSettings ResolveAimHelper(ushort weaponId) =>
        WeaponSettingsResolver.Resolve(ActiveSettings.AimHelper, weaponId);

    private void ReloadFromStore()
    {
        lock (_lock)
        {
            _activeProfile = _configManager.GetActiveProfile();
            _activeSettings = _activeProfile.Settings;
            _current = _configManager.BuildToolkitOptions();
        }

        ConfigChanged?.Invoke();
    }
}

public sealed class ActiveWeaponTracker
{
    private volatile ushort _weaponId;

    public ushort WeaponId => _weaponId;

    public void Update(ProcessMemory memory, GameOffsets offsets, nint clientBase)
    {
        if (!memory.IsAttached || clientBase == nint.Zero)
        {
            _weaponId = 0;
            return;
        }

        var localPawn = memory.ReadPtr(clientBase + offsets.DwLocalPlayerPawn);
        if (localPawn == nint.Zero)
        {
            _weaponId = 0;
            return;
        }

        var weaponServices = memory.ReadPtr(localPawn + offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
        {
            _weaponId = 0;
            return;
        }

        var entityList = memory.ReadPtr(clientBase + offsets.DwEntityList);
        if (entityList == nint.Zero)
        {
            _weaponId = 0;
            return;
        }

        var activeHandle = memory.Read<uint>(weaponServices + offsets.M_hActiveWeapon);
        if (activeHandle == 0)
        {
            _weaponId = 0;
            return;
        }

        var weaponEntity = ResolveEntityFromHandle(memory, entityList, activeHandle);
        if (weaponEntity == nint.Zero)
        {
            _weaponId = 0;
            return;
        }

        var itemAddr = memory.ReadPtr(weaponEntity + offsets.M_AttributeManager + offsets.M_Item);
        if (itemAddr == nint.Zero)
        {
            _weaponId = 0;
            return;
        }

        _weaponId = memory.Read<ushort>(itemAddr + offsets.M_iItemDefinitionIndex);
    }

    private static nint ResolveEntityFromHandle(ProcessMemory memory, nint entityList, uint handle)
    {
        var index = (int)(handle & 0x7FFF);
        var listEntry = memory.ReadPtr(entityList + 0x8 * (index >> 9) + 0x10);
        if (listEntry == nint.Zero)
            return nint.Zero;

        return memory.ReadPtr(listEntry + GameOffsets.EntitySpacings[0] * (index & 0x1FF));
    }
}
