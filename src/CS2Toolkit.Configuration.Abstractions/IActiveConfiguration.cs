namespace CS2Toolkit.Configuration.Abstractions;

public interface IActiveConfiguration
{
    ToolkitSettings Current { get; }
    ResolvedWeaponSettings ResolveWeapon(ushort weaponId);
    void Refresh();
}
