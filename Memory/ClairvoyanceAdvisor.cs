using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;

namespace Cs2Toolkit.Memory;

public sealed class ClairvoyanceAdvisor
{
    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly ClairvoyanceOptions _options;

    public ClairvoyanceAdvisor(ProcessMemory memory, GameOffsets offsets, ClairvoyanceOptions options)
    {
        _memory = memory;
        _offsets = offsets;
        _options = options;
    }

    public IReadOnlyList<string> ResolveTips(
        nint clientBase,
        nint entityList,
        nint localPawn,
        int localTeam,
        IReadOnlyList<PlayerInfo> players,
        BombInfo bomb,
        BombSitesInfo bombSites)
    {
        if (entityList == nint.Zero || localPawn == nint.Zero || localTeam == 0)
            return ["No tips yet..."];

        var tips = new List<string>();
        var localPosition = ReadPosition(localPawn);

        if (ShouldSuggestReload(localPawn, entityList))
            tips.Add("You should reload");

        if (IsEnemyNearby(localPosition, entityList, players, localTeam))
            tips.Add("You should be sneaky");

        if (bombSites.IsValid)
        {
            if (localTeam == GameOffsets.TeamCounterTerrorist)
            {
                var enemySiteTip = PredictEnemyRotation(bombSites, entityList, players, localTeam);
                if (enemySiteTip is not null)
                    tips.Add(enemySiteTip);

                var plantingTip = PredictPlantingSite(clientBase, entityList, players, bomb, bombSites);
                if (plantingTip is not null)
                    tips.Add(plantingTip);
            }
            else if (localTeam == GameOffsets.TeamTerrorist)
            {
                var plantSiteTip = SuggestPlantSite(bombSites, entityList, players, localTeam, bomb);
                if (plantSiteTip is not null)
                    tips.Add(plantSiteTip);

                var defuseWarning = WarnEnemyApproachingDefuse(
                    bombSites, localPosition, entityList, players, localTeam, bomb);
                if (defuseWarning is not null)
                    tips.Add(defuseWarning);
            }
        }

        return tips.Count > 0 ? tips : ["No tips yet..."];
    }

    private bool ShouldSuggestReload(nint localPawn, nint entityList)
    {
        var weaponServices = _memory.ReadPtr(localPawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var activeHandle = _memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return false;

        var weapon = ResolveEntityFromHandle(entityList, activeHandle);
        if (weapon == nint.Zero)
            return false;

        var weaponId = GetWeaponDefinitionIndex(weapon);
        if (weaponId is GameOffsets.WeaponC4 or GameOffsets.WeaponKnife or GameOffsets.WeaponKnifeT)
            return false;

        var clip = _memory.Read<int>(weapon + _offsets.M_iClip1);
        return clip >= 0 && clip <= _options.LowAmmoClipThreshold;
    }

    private bool IsEnemyNearby(
        Vector3 localPosition,
        nint entityList,
        IReadOnlyList<PlayerInfo> players,
        int localTeam)
    {
        if (!localPosition.IsValid)
            return false;

        foreach (var player in players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == localTeam)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var enemyPosition = ReadPosition(pawn);
            if (!enemyPosition.IsValid)
                continue;

            if (localPosition.DistanceTo(enemyPosition) <= _options.EnemyCloseDistanceUnits)
                return true;
        }

        return false;
    }

    private string? PredictEnemyRotation(
        BombSitesInfo bombSites,
        nint entityList,
        IReadOnlyList<PlayerInfo> players,
        int localTeam)
    {
        var (nearA, nearB) = CountEnemiesNearSites(bombSites, entityList, players, localTeam);

        if (nearA > nearB && nearA > 0)
            return "They're probably going A";

        if (nearB > nearA && nearB > 0)
            return "They're probably going B";

        return null;
    }

    private string? SuggestPlantSite(
        BombSitesInfo bombSites,
        nint entityList,
        IReadOnlyList<PlayerInfo> players,
        int localTeam,
        BombInfo bomb)
    {
        if (bomb.Status is BombStatus.Planting or BombStatus.Planted or BombStatus.Defusing)
            return null;

        var (nearA, nearB) = CountEnemiesNearSites(bombSites, entityList, players, localTeam);

        if (nearA > nearB && nearA > 0)
            return "We should plant on site A";

        if (nearB > nearA && nearB > 0)
            return "We should plant on site B";

        return null;
    }

    private string? WarnEnemyApproachingDefuse(
        BombSitesInfo bombSites,
        Vector3 localPosition,
        nint entityList,
        IReadOnlyList<PlayerInfo> players,
        int localTeam,
        BombInfo bomb)
    {
        if (bomb.Status != BombStatus.Planted || string.IsNullOrEmpty(bomb.Site))
            return null;

        if (!localPosition.IsValid)
            return null;

        var siteCenter = bomb.Site == "A" ? bombSites.CenterA : bombSites.CenterB;
        if (!siteCenter.IsValid)
            return null;

        var radius = _options.BombsiteEnemyRadiusUnits;
        if (localPosition.DistanceTo2D(siteCenter) <= radius)
            return null;

        foreach (var player in players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == localTeam)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var position = ReadPosition(pawn);
            if (!position.IsValid)
                continue;

            if (position.DistanceTo2D(siteCenter) <= radius)
                return "They're about to defuse...";
        }

        return null;
    }

    private (int NearA, int NearB) CountEnemiesNearSites(
        BombSitesInfo bombSites,
        nint entityList,
        IReadOnlyList<PlayerInfo> players,
        int localTeam)
    {
        var nearA = 0;
        var nearB = 0;
        var radius = _options.BombsiteEnemyRadiusUnits;

        foreach (var player in players)
        {
            if (player.IsLocalPlayer || !player.IsAlive || player.Team == localTeam)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var position = ReadPosition(pawn);
            if (!position.IsValid)
                continue;

            if (position.DistanceTo2D(bombSites.CenterA) <= radius)
                nearA++;
            if (position.DistanceTo2D(bombSites.CenterB) <= radius)
                nearB++;
        }

        return (nearA, nearB);
    }

    private string? PredictPlantingSite(
        nint clientBase,
        nint entityList,
        IReadOnlyList<PlayerInfo> players,
        BombInfo bomb,
        BombSitesInfo bombSites)
    {
        if (bomb.Status == BombStatus.Planting && !string.IsNullOrEmpty(bomb.Site))
            return $"They're probably planting {bomb.Site}";

        foreach (var player in players)
        {
            if (player.Team != GameOffsets.TeamTerrorist || !player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero || !HasBombInInventoryOrActive(pawn, entityList))
                continue;

            if (_memory.Read<byte>(pawn + _offsets.M_bInBombZone) != 0)
            {
                var site = ResolveCarrierPlantSiteLabel(pawn, entityList, bombSites);
                if (site is not null)
                    return $"They're probably planting {site}";
            }
        }

        if (bomb.Status == BombStatus.OnGround)
        {
            var bombPosition = ResolveBombWorldPosition(clientBase, entityList, players, bomb);
            if (bombPosition is not null && bombPosition.Value.IsValid)
            {
                var site = bombSites.LabelForPosition(bombPosition.Value);
                if (site is not null)
                    return $"They're probably planting {site}";
            }
        }

        return null;
    }

    private string? ResolveCarrierPlantSiteLabel(nint pawn, nint entityList, BombSitesInfo bombSites)
    {
        var weaponServices = _memory.ReadPtr(pawn + _offsets.M_pWeaponServices);
        if (weaponServices != nint.Zero)
        {
            var activeHandle = _memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
            if (activeHandle is not (0 or 0xFFFFFFFF))
            {
                var weapon = ResolveEntityFromHandle(entityList, activeHandle);
                if (weapon != nint.Zero)
                {
                    var weaponSite = bombSites.LabelForPosition(
                        BombSiteHelper.ReadEntityPosition(_memory, _offsets, weapon));
                    if (weaponSite is not null)
                        return weaponSite;
                }
            }
        }

        return bombSites.LabelForPosition(ReadPosition(pawn));
    }

    private Vector3? ResolveBombWorldPosition(
        nint clientBase,
        nint entityList,
        IReadOnlyList<PlayerInfo> players,
        BombInfo bomb)
    {
        if (bomb.Status == BombStatus.OnGround)
        {
            var weaponC4 = _memory.ReadPtr(clientBase + _offsets.DwWeaponC4);
            if (weaponC4 != nint.Zero)
            {
                var position = BombSiteHelper.ReadEntityPosition(_memory, _offsets, weaponC4);
                if (position.IsValid)
                    return position;
            }
        }

        foreach (var player in players)
        {
            if (player.Team != GameOffsets.TeamTerrorist || !player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (!HasBombInInventoryOrActive(pawn, entityList))
                continue;

            var position = ReadPosition(pawn);
            if (position.IsValid)
                return position;
        }

        return null;
    }

    private bool HasBombInInventoryOrActive(nint pawn, nint entityList) =>
        HasActiveWeapon(pawn, entityList, GameOffsets.WeaponC4)
        || HasWeaponInInventory(pawn, entityList, GameOffsets.WeaponC4);

    private Vector3 ReadPosition(nint pawn) =>
        BombSiteHelper.ReadEntityPosition(_memory, _offsets, pawn);

    private nint ResolvePawnForPlayer(nint entityList, int index)
    {
        var controller = ResolveControllerFromIndex(entityList, index);
        if (controller == nint.Zero)
            return nint.Zero;

        var pawnHandle = _memory.Read<uint>(controller + _offsets.M_hPlayerPawn);
        if (pawnHandle is 0 or 0xFFFFFFFF)
            return nint.Zero;

        return ResolvePawnFromHandle(entityList, pawnHandle);
    }

    private nint ResolveControllerFromIndex(nint entityList, int index)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
            if (listEntry == nint.Zero)
                continue;

            var controller = _memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
            if (controller != nint.Zero)
                return controller;
        }

        return nint.Zero;
    }

    private nint ResolveEntityFromHandle(nint entityList, uint handle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var entity = ResolvePawnFromHandle(entityList, handle, spacing);
            if (entity != nint.Zero)
                return entity;
        }

        return nint.Zero;
    }

    private nint ResolvePawnFromHandle(nint entityList, uint pawnHandle)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var pawn = ResolvePawnFromHandle(entityList, pawnHandle, spacing);
            if (pawn != nint.Zero)
                return pawn;
        }

        return nint.Zero;
    }

    private nint ResolvePawnFromHandle(nint entityList, uint pawnHandle, int spacing)
    {
        var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return _memory.ReadPtr(listEntry + (nint)(spacing * (pawnHandle & 0x1FF)));
    }

    private bool HasActiveWeapon(nint pawn, nint entityList, ushort weaponId)
    {
        var weaponServices = _memory.ReadPtr(pawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var activeHandle = _memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return false;

        var weapon = ResolveEntityFromHandle(entityList, activeHandle);
        return weapon != nint.Zero && GetWeaponDefinitionIndex(weapon) == weaponId;
    }

    private bool HasWeaponInInventory(nint pawn, nint entityList, ushort weaponId)
    {
        var weaponServices = _memory.ReadPtr(pawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var count = _memory.Read<int>(weaponServices + _offsets.M_hMyWeapons);
        if (count <= 0)
            return false;

        var dataPtr = _memory.ReadPtr(weaponServices + _offsets.M_hMyWeapons + 0x8);
        if (dataPtr == nint.Zero)
            return false;

        count = Math.Min(count, 64);
        for (var i = 0; i < count; i++)
        {
            var handle = _memory.Read<uint>(dataPtr + (nint)(i * 4));
            if (handle is 0 or 0xFFFFFFFF)
                continue;

            var weapon = ResolveEntityFromHandle(entityList, handle);
            if (weapon != nint.Zero && GetWeaponDefinitionIndex(weapon) == weaponId)
                return true;
        }

        return false;
    }

    private ushort GetWeaponDefinitionIndex(nint weaponEntity) =>
        _memory.Read<ushort>(
            weaponEntity
            + _offsets.M_AttributeManager
            + _offsets.M_Item
            + _offsets.M_iItemDefinitionIndex);
}
