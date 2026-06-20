using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Memory;

internal sealed class EntitySnapshotReader
{
    private const byte LifeStateDead = 2;
    private const uint PlayerDisconnected = 4;
    private const uint PlayerNeverConnected = 0xFFFFFFFF;

    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly ClairvoyanceAdvisorStub _clairvoyanceAdvisor;

    internal EntitySnapshotReader(ProcessMemory memory, GameOffsets offsets, ClairvoyanceOptionsStub clairvoyanceOptions)
    {
        _memory = memory;
        _offsets = offsets;
        _clairvoyanceAdvisor = new ClairvoyanceAdvisorStub(memory, offsets, clairvoyanceOptions);
    }

    public LegacyMemoryState ReadState()
    {
        if (!_memory.IsAttached)
            return LegacyMemoryState.Detached;

        var clientBase = _memory.ClientBase;
        var entityList = _memory.ReadPtr(clientBase + _offsets.DwEntityList);
        var localPawn = _memory.ReadPtr(clientBase + _offsets.DwLocalPlayerPawn);
        var localController = _memory.ReadPtr(clientBase + _offsets.DwLocalPlayerController);

        if (entityList == nint.Zero)
            return UnmatchedState(localPawn, localController, round: ReadLegacyRoundInfo(clientBase));

        var localTeam = ResolveTeam(localPawn, localController, entityList);
        var round = ReadLegacyRoundInfo(clientBase);
        var isInMatch = IsCurrentlyInMatch(clientBase, localPawn, localTeam);

        if (!isInMatch)
            return UnmatchedState(localPawn, localController, localTeam, round);

        var players = CollectPlayers(entityList, localController);
        EnrichPlayerEspData(entityList, localTeam, players);
        var stats = ResolveStats(clientBase, localController, localTeam, players);
        var bombSites = BombSiteHelper.TryReadSites(_memory, _offsets, entityList);
        var bomb = ResolveLegacyBombInfo(clientBase, entityList, players, bombSites);
        var tips = _clairvoyanceAdvisor.ResolveTips(
            clientBase, entityList, localPawn, localTeam, players, bomb, bombSites);

        return new LegacyMemoryState
        {
            IsAttached = true,
            IsInGame = localPawn != nint.Zero,
            IsInMatch = true,
            LocalTeam = localTeam,
            Players = players,
            EnemiesAlive = stats.EnemiesAlive,
            EnemiesDead = stats.EnemiesDead,
            TeammatesAlive = stats.TeammatesAlive,
            TeammatesDead = stats.TeammatesDead,
            Round = round,
            Bomb = bomb,
            BombSites = bombSites,
            ClairvoyanceTips = tips
        };
    }

    private LegacyBombInfo ResolveLegacyBombInfo(
        nint clientBase,
        nint entityList,
        List<LegacyPlayerInfo> players,
        LegacyBombSitesInfo bombSites)
    {
        var gameRules = _memory.ReadPtr(clientBase + _offsets.DwGameRules);
        if (gameRules == nint.Zero)
            return LegacyBombInfo.Hidden;

        if (_memory.Read<byte>(gameRules + _offsets.M_bBombPlanted) != 0)
            return ResolvePlantedLegacyBombInfo(clientBase, entityList, players, bombSites);

        var planting = TryResolvePlanting(entityList, players, bombSites);
        if (planting is not null)
            return planting;

        if (_memory.Read<byte>(gameRules + _offsets.M_bBombDropped) != 0)
            return new LegacyBombInfo { Status = LegacyBombStatus.OnGround };

        var hasCarried = false;

        foreach (var player in players)
        {
            if (player.Team != GameOffsets.TeamTerrorist || !player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (HasActiveWeapon(pawn, entityList, GameOffsets.WeaponC4))
                return new LegacyBombInfo { Status = LegacyBombStatus.Equipped };

            if (HasWeaponInInventory(pawn, entityList, GameOffsets.WeaponC4))
                hasCarried = true;
        }

        return hasCarried
            ? new LegacyBombInfo { Status = LegacyBombStatus.Carried }
            : LegacyBombInfo.Hidden;
    }

    private LegacyBombInfo ResolvePlantedLegacyBombInfo(
        nint clientBase,
        nint entityList,
        List<LegacyPlayerInfo> players,
        LegacyBombSitesInfo bombSites)
    {
        var plantedC4 = BombSiteHelper.ResolvePlantedC4Entity(_memory, _offsets, clientBase);
        var curtime = ReadCurtime(clientBase, plantedC4);
        string? site = null;
        int? timeLeft = null;
        LegacyVector3? worldPosition = null;

        if (plantedC4 != nint.Zero)
        {
            var position = BombSiteHelper.ReadEntityPosition(_memory, _offsets, plantedC4);
            if (position.IsValid)
                worldPosition = position;

            site = ResolveSiteLabel(plantedC4, bombSites);
            if (string.IsNullOrEmpty(site))
                site = ResolveSiteFromBombIndex(plantedC4);

            if (worldPosition is not { IsValid: true })
                worldPosition = BombSiteHelper.TryResolveSiteCenter(site, bombSites);
            var blowTime = _memory.Read<float>(plantedC4 + _offsets.M_flC4Blow);
            var timerLength = _memory.Read<float>(plantedC4 + _offsets.M_flTimerLength);
            timeLeft = ResolveTimedEventRemaining(blowTime, curtime, timerLength, defaultMaxSeconds: 45f);

            var beingDefused = _memory.Read<byte>(plantedC4 + _offsets.M_bBeingDefused) != 0
                || IsAnyPlayerDefusing(entityList, players);

            if (beingDefused)
            {
                var cannotBeDefused = _memory.Read<byte>(plantedC4 + _offsets.M_bCannotBeDefused) != 0;
                var defuserHandle = _memory.Read<uint>(plantedC4 + _offsets.M_hBombDefuser);
                var defuseTime = ResolveDefuseTimeRemaining(
                    plantedC4, entityList, players, defuserHandle, curtime);
                var willSucceed = ResolveWillDefuseSucceed(
                    cannotBeDefused, plantedC4, blowTime, curtime, timeLeft, defuseTime);

                return new LegacyBombInfo
                {
                    Status = LegacyBombStatus.Defusing,
                    Site = site,
                    TimeLeftSeconds = timeLeft,
                    WorldPosition = worldPosition,
                    HasDefuseKit = ResolveDefuserHasKit(entityList, players, defuserHandle),
                    DefuseTimeSeconds = defuseTime,
                    WillDefuseSucceed = willSucceed
                };
            }
        }

        return new LegacyBombInfo
        {
            Status = LegacyBombStatus.Planted,
            Site = site,
            TimeLeftSeconds = timeLeft,
            WorldPosition = worldPosition
        };
    }

    private LegacyBombInfo? TryResolvePlanting(nint entityList, List<LegacyPlayerInfo> players, LegacyBombSitesInfo bombSites)
    {
        foreach (var player in players)
        {
            if (player.Team != GameOffsets.TeamTerrorist || !player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero || !IsPlanting(pawn, entityList))
                continue;

            var site = ResolvePlantingSiteLabel(pawn, entityList, bombSites);

            return new LegacyBombInfo
            {
                Status = LegacyBombStatus.Planting,
                Site = site
            };
        }

        return null;
    }

    private string? ResolvePlantingSiteLabel(nint pawn, nint entityList, LegacyBombSitesInfo bombSites)
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

        return bombSites.LabelForPosition(BombSiteHelper.ReadEntityPosition(_memory, _offsets, pawn));
    }

    private string? ResolveSiteLabel(nint entity, LegacyBombSitesInfo bombSites) =>
        bombSites.LabelForPosition(BombSiteHelper.ReadEntityPosition(_memory, _offsets, entity));

    private string? ResolveSiteFromBombIndex(nint plantedC4) =>
        _memory.Read<int>(plantedC4 + _offsets.M_nBombSite) switch
        {
            0 => "A",
            1 => "B",
            _ => null
        };

    private bool IsPlanting(nint pawn, nint entityList)
    {
        var weaponServices = _memory.ReadPtr(pawn + _offsets.M_pWeaponServices);
        if (weaponServices == nint.Zero)
            return false;

        var activeHandle = _memory.Read<uint>(weaponServices + _offsets.M_hActiveWeapon);
        if (activeHandle is 0 or 0xFFFFFFFF)
            return false;

        var weapon = ResolveEntityFromHandle(entityList, activeHandle);
        if (weapon == nint.Zero || GetWeaponDefinitionIndex(weapon) != GameOffsets.WeaponC4)
            return false;

        return _memory.Read<byte>(weapon + _offsets.M_bStartedArming) != 0
            || _memory.Read<byte>(weapon + _offsets.M_bIsPlantingViaUse) != 0;
    }

    private bool IsAnyPlayerDefusing(nint entityList, List<LegacyPlayerInfo> players)
    {
        foreach (var player in players)
        {
            if (player.Team != GameOffsets.TeamCounterTerrorist || !player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (_memory.Read<byte>(pawn + _offsets.M_bIsDefusing) != 0)
                return true;

            var action = _memory.Read<int>(pawn + _offsets.M_iBlockingUseActionInProgress);
            if (action is GameOffsets.BlockingUseActionDefuseDefault or GameOffsets.BlockingUseActionDefuseWithKit)
                return true;
        }

        return false;
    }

    private bool? ResolveDefuserHasKit(nint entityList, List<LegacyPlayerInfo> players, uint defuserHandle)
    {
        if (defuserHandle is not (0 or 0xFFFFFFFF))
        {
            var defuserPawn = ResolveEntityFromHandle(entityList, defuserHandle);
            if (defuserPawn != nint.Zero)
            {
                var action = _memory.Read<int>(defuserPawn + _offsets.M_iBlockingUseActionInProgress);
                if (action == GameOffsets.BlockingUseActionDefuseWithKit)
                    return true;
                if (action == GameOffsets.BlockingUseActionDefuseDefault)
                    return false;

                var itemServices = _memory.ReadPtr(defuserPawn + _offsets.M_pItemServices);
                if (itemServices != nint.Zero)
                    return _memory.Read<byte>(itemServices + _offsets.M_bHasDefuser) != 0;
            }
        }

        foreach (var player in players)
        {
            if (player.Team != GameOffsets.TeamCounterTerrorist || !player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            var action = _memory.Read<int>(pawn + _offsets.M_iBlockingUseActionInProgress);
            if (action == GameOffsets.BlockingUseActionDefuseWithKit)
                return true;
            if (action == GameOffsets.BlockingUseActionDefuseDefault)
                return false;

            if (_memory.Read<byte>(pawn + _offsets.M_bIsDefusing) == 0)
                continue;

            var controller = ResolveControllerForPlayer(entityList, player.Index);
            if (controller != nint.Zero && _memory.Read<byte>(controller + _offsets.M_bPawnHasDefuser) != 0)
                return true;

            var itemServices = _memory.ReadPtr(pawn + _offsets.M_pItemServices);
            if (itemServices != nint.Zero && _memory.Read<byte>(itemServices + _offsets.M_bHasDefuser) != 0)
                return true;

            return false;
        }

        return null;
    }

    private nint ResolvePawnForPlayer(nint entityList, int index)
    {
        var controller = ResolveControllerForPlayer(entityList, index);
        if (controller == nint.Zero)
            return nint.Zero;

        var pawnHandle = _memory.Read<uint>(controller + _offsets.M_hPlayerPawn);
        if (pawnHandle is 0 or 0xFFFFFFFF)
            return nint.Zero;

        return ResolvePawnFromHandle(entityList, pawnHandle);
    }

    private float ReadCurtime(nint clientBase, nint plantedC4)
    {
        var globalVars = _memory.ReadPtr(clientBase + _offsets.DwGlobalVars);
        if (globalVars == nint.Zero)
            return 0;

        float? blowTime = null;
        var maxRemaining = 45f;
        if (plantedC4 != nint.Zero)
        {
            blowTime = _memory.Read<float>(plantedC4 + _offsets.M_flC4Blow);
            var timerLength = _memory.Read<float>(plantedC4 + _offsets.M_flTimerLength);
            if (timerLength is > 0 and <= 60)
                maxRemaining = timerLength + 2f;
        }

        foreach (var offset in GameOffsets.GlobalVarsCurtimeCandidates)
        {
            var candidate = _memory.Read<float>(globalVars + offset);
            if (candidate <= 0)
                continue;

            if (blowTime is not > 0)
                return candidate;

            var remaining = blowTime.Value - candidate;
            if (remaining >= -0.5f && remaining <= maxRemaining)
                return candidate;
        }

        var preferred = _memory.Read<float>(globalVars + GameOffsets.GlobalVarsCurtime);
        if (preferred > 0)
            return preferred;

        return _memory.Read<float>(globalVars + GameOffsets.GlobalVarsCurtimeAlt);
    }

    private int? ResolveTimedEventRemaining(
        float targetTime,
        float curtime,
        float configuredLength,
        float defaultMaxSeconds)
    {
        if (targetTime <= 0 || curtime <= 0)
            return null;

        var remaining = targetTime - curtime;
        var maxSeconds = configuredLength is > 0 and <= 60 ? configuredLength + 2f : defaultMaxSeconds;
        if (remaining < -0.5f || remaining > maxSeconds)
            return null;

        return SecondsRemaining(targetTime, curtime);
    }

    private int? ResolveDefuseTimeRemaining(
        nint plantedC4,
        nint entityList,
        List<LegacyPlayerInfo> players,
        uint defuserHandle,
        float curtime)
    {
        if (plantedC4 != nint.Zero)
        {
            var defuseCountDown = _memory.Read<float>(plantedC4 + _offsets.M_flDefuseCountDown);
            var defuseLength = _memory.Read<float>(plantedC4 + _offsets.M_flDefuseLength);
            var lastDefuseTime = _memory.Read<float>(plantedC4 + _offsets.M_fLastDefuseTime);
            var remaining = ResolveTimedEventRemaining(
                defuseCountDown, curtime, defuseLength, defaultMaxSeconds: 12f);
            if (remaining > 0)
                return remaining;

            if (defuseLength > 0 && lastDefuseTime > 0)
            {
                remaining = ResolveTimedEventRemaining(
                    lastDefuseTime + defuseLength, curtime, defuseLength, defaultMaxSeconds: 12f);
                if (remaining > 0)
                    return remaining;
            }
        }

        var defuserPawn = ResolveDefuserPawn(entityList, players, defuserHandle);
        if (defuserPawn != nint.Zero)
        {
            var progressRemaining = ResolveProgressBarRemaining(defuserPawn, curtime);
            if (progressRemaining > 0)
                return progressRemaining;
        }

        return 0;
    }

    private nint ResolveDefuserPawn(nint entityList, List<LegacyPlayerInfo> players, uint defuserHandle)
    {
        if (defuserHandle is not (0 or 0xFFFFFFFF))
        {
            var defuserPawn = ResolveEntityFromHandle(entityList, defuserHandle);
            if (defuserPawn != nint.Zero)
                return defuserPawn;
        }

        foreach (var player in players)
        {
            if (player.Team != GameOffsets.TeamCounterTerrorist || !player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (_memory.Read<byte>(pawn + _offsets.M_bIsDefusing) != 0)
                return pawn;

            var action = _memory.Read<int>(pawn + _offsets.M_iBlockingUseActionInProgress);
            if (action is GameOffsets.BlockingUseActionDefuseDefault or GameOffsets.BlockingUseActionDefuseWithKit)
                return pawn;
        }

        return nint.Zero;
    }

    private bool? ResolveWillDefuseSucceed(
        bool cannotBeDefused,
        nint plantedC4,
        float blowTime,
        float curtime,
        int? bombTimeRemaining,
        int? defuseTimeRemaining)
    {
        if (defuseTimeRemaining is not > 0)
            return null;

        if (bombTimeRemaining is > 0)
            return bombTimeRemaining >= defuseTimeRemaining;

        if (curtime > 0 && IsFutureGameTime(blowTime, curtime))
        {
            if (plantedC4 != nint.Zero)
            {
                var defuseEndTime = ResolveDefuseEndTime(plantedC4, curtime);
                if (defuseEndTime > 0)
                    return defuseEndTime <= blowTime;
            }

            return SecondsRemaining(blowTime, curtime) >= defuseTimeRemaining;
        }

        return cannotBeDefused ? false : null;
    }

    private float ResolveDefuseEndTime(nint plantedC4, float curtime)
    {
        var defuseCountDown = _memory.Read<float>(plantedC4 + _offsets.M_flDefuseCountDown);
        if (IsFutureGameTime(defuseCountDown, curtime))
            return defuseCountDown;

        var lastDefuseTime = _memory.Read<float>(plantedC4 + _offsets.M_fLastDefuseTime);
        var defuseLength = _memory.Read<float>(plantedC4 + _offsets.M_flDefuseLength);
        if (lastDefuseTime > 0 && defuseLength > 0)
        {
            var defuseEndTime = lastDefuseTime + defuseLength;
            if (IsFutureGameTime(defuseEndTime, curtime))
                return defuseEndTime;
        }

        return 0;
    }

    private static bool IsFutureGameTime(float timestamp, float curtime) =>
        timestamp > curtime && timestamp < curtime + 120f;

    private int ResolveProgressBarRemaining(nint pawn, float curtime)
    {
        var duration = _memory.Read<int>(pawn + _offsets.M_iProgressBarDuration);
        var startTime = _memory.Read<float>(pawn + _offsets.M_flProgressBarStartTime);
        if (duration <= 0 || startTime <= 0)
            return 0;

        var remaining = SecondsRemaining(startTime + duration, curtime);
        if (remaining is > 0 and <= 12)
            return remaining;

        remaining = SecondsRemaining(startTime + duration / 10f, curtime);
        if (remaining is > 0 and <= 12)
            return remaining;

        return 0;
    }

    private static int SecondsRemaining(float targetTime, float curtime) =>
        Math.Max(0, (int)MathF.Floor(targetTime - curtime));

    private nint ResolveControllerForPlayer(nint entityList, int index)
    {
        foreach (var spacing in GameOffsets.EntitySpacings)
        {
            var controller = ResolveControllerFromIndex(entityList, index, spacing);
            if (controller == nint.Zero || !LooksLikePlayerController(controller))
                continue;

            return controller;
        }

        return nint.Zero;
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

    private ushort GetWeaponDefinitionIndex(nint weaponEntity)
    {
        return _memory.Read<ushort>(
            weaponEntity
            + _offsets.M_AttributeManager
            + _offsets.M_Item
            + _offsets.M_iItemDefinitionIndex);
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

    private void EnrichPlayerEspData(nint entityList, int localTeam, List<LegacyPlayerInfo> players)
    {
        if (localTeam is not (GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist))
            return;

        var friendlyIndices = new HashSet<int>();
        var localPlayerIndex = -1;

        foreach (var player in players)
        {
            if (player.IsLocalPlayer)
                localPlayerIndex = player.Index;

            if (player.Team == localTeam)
                friendlyIndices.Add(player.Index);
        }

        foreach (var player in players)
        {
            if (!player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            player.Bones = BoneReader.TryReadSkeleton(_memory, _offsets, pawn);
            player.IsSpottedByTeam = IsSpottedByFriendlyTeam(pawn, friendlyIndices);
            player.IsVisibleToLocalPlayer = IsSpottedByPlayer(pawn, localPlayerIndex);
        }
    }

    private bool IsSpottedByPlayer(nint pawn, int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 64 || _offsets.M_entitySpottedState == nint.Zero)
            return false;

        var spottedState = pawn + _offsets.M_entitySpottedState;
        var maskIndex = playerIndex / 32;
        var bit = playerIndex % 32;
        var mask = _memory.Read<uint>(spottedState + GameOffsets.EntitySpottedState_bSpottedByMask + (nint)(maskIndex * 4));
        return (mask & (1u << bit)) != 0;
    }

    private bool IsSpottedByFriendlyTeam(nint pawn, HashSet<int> friendlyIndices)
    {
        if (_offsets.M_entitySpottedState == nint.Zero)
            return false;

        var spottedState = pawn + _offsets.M_entitySpottedState;

        if (_memory.Read<byte>(spottedState + GameOffsets.EntitySpottedState_bSpotted) != 0)
            return true;

        foreach (var index in friendlyIndices)
        {
            if (index < 0 || index >= 64)
                continue;

            var maskIndex = index / 32;
            var bit = index % 32;
            var mask = _memory.Read<uint>(spottedState + GameOffsets.EntitySpottedState_bSpottedByMask + (nint)(maskIndex * 4));
            if ((mask & (1u << bit)) != 0)
                return true;
        }

        return false;
    }

    private (int EnemiesAlive, int EnemiesDead, int TeammatesAlive, int TeammatesDead) ResolveStats(
        nint clientBase,
        nint localController,
        int localTeam,
        List<LegacyPlayerInfo> players)
    {
        var gameRules = _memory.ReadPtr(clientBase + _offsets.DwGameRules);

        var enemiesAlive = 0;
        var enemiesDead = 0;
        var teammatesAlive = 0;
        var teammatesDead = 0;

        var enemies = players.Where(p => !p.IsLocalPlayer && p.Team != localTeam).ToList();
        var teammates = players.Where(p => !p.IsLocalPlayer && p.Team == localTeam).ToList();

        enemiesDead = enemies.Count(p => !p.IsAlive);
        teammatesDead = teammates.Count(p => !p.IsAlive);

        if (players.Count > 0)
        {
            enemiesAlive = enemies.Count(p => p.IsAlive);
            teammatesAlive = teammates.Count(p => p.IsAlive);
        }
        else if (gameRules != nint.Zero)
        {
            var ctAlive = _memory.Read<int>(gameRules + _offsets.M_iMatchStatsPlayersAliveCt);
            var tAlive = _memory.Read<int>(gameRules + _offsets.M_iMatchStatsPlayersAliveT);
            var localAlive = localController != nint.Zero
                && _memory.Read<byte>(localController + _offsets.M_bPawnIsAlive) != 0;

            if (localTeam == GameOffsets.TeamCounterTerrorist)
            {
                enemiesAlive = tAlive;
                teammatesAlive = Math.Max(0, ctAlive - (localAlive ? 1 : 0));
            }
            else if (localTeam == GameOffsets.TeamTerrorist)
            {
                enemiesAlive = ctAlive;
                teammatesAlive = Math.Max(0, tAlive - (localAlive ? 1 : 0));
            }
        }

        return (enemiesAlive, enemiesDead, teammatesAlive, teammatesDead);
    }

    private List<LegacyPlayerInfo> CollectPlayers(nint entityList, nint localController)
    {
        var players = new List<LegacyPlayerInfo>();
        var seenControllers = new HashSet<nint>();

        for (var index = 1; index <= GameOffsets.MaxPlayerIndex; index++)
        {
            foreach (var spacing in GameOffsets.EntitySpacings)
            {
                var controller = ResolveControllerFromIndex(entityList, index, spacing);
                if (controller == nint.Zero || seenControllers.Contains(controller))
                    continue;

                if (TryAddPlayer(players, entityList, controller, localController, index))
                {
                    seenControllers.Add(controller);
                    break;
                }
            }
        }

        var controllerListHead = _memory.ReadPtr(entityList + 0x10);
        if (controllerListHead != nint.Zero && players.Count < GameOffsets.MaxPlayerIndex)
        {
            foreach (var spacing in GameOffsets.EntitySpacings)
            {
                for (var slot = 1; slot <= GameOffsets.MaxPlayerIndex; slot++)
                {
                    var controller = _memory.ReadPtr(controllerListHead + (nint)(spacing * slot));
                    if (controller == nint.Zero || seenControllers.Contains(controller))
                        continue;

                    if (TryAddPlayer(players, entityList, controller, localController, slot))
                        seenControllers.Add(controller);
                }
            }
        }

        return players;
    }

    private bool TryAddPlayer(
        List<LegacyPlayerInfo> players,
        nint entityList,
        nint controller,
        nint localController,
        int index)
    {
        if (!LooksLikePlayerController(controller))
            return false;

        var name = ReadPlayerName(controller, index);
        if (!IsValidPlayerName(name))
            return false;

        var pawnHandle = _memory.Read<uint>(controller + _offsets.M_hPlayerPawn);
        nint pawn = nint.Zero;
        if (pawnHandle is not (0 or 0xFFFFFFFF))
            pawn = ResolvePawnFromHandle(entityList, pawnHandle);

        var team = ReadTeam(controller, pawn);
        if (team is not (GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist))
            return false;

        var (isAlive, health) = ResolveAliveState(controller, pawn);
        var isLocal = controller == localController;
        var worldPosition = ReadPawnWorldPosition(pawn);

        players.Add(new LegacyPlayerInfo
        {
            Index = index,
            Name = name,
            Team = team,
            Health = health,
            IsAlive = isAlive,
            IsLocalPlayer = isLocal,
            WorldPosition = worldPosition
        });

        return true;
    }

    private bool LooksLikePlayerController(nint controller)
    {
        var pawnHandle = _memory.Read<uint>(controller + _offsets.M_hPlayerPawn);
        if (pawnHandle is 0 or 0xFFFFFFFF)
            return false;

        if (_offsets.M_iConnected != nint.Zero)
        {
            var connected = _memory.Read<uint>(controller + _offsets.M_iConnected);
            if (connected is PlayerDisconnected or PlayerNeverConnected)
                return false;
        }

        return true;
    }

    private static bool IsValidPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (name.StartsWith("Player ", StringComparison.Ordinal))
            return false;

        if (name.Contains("prefab", StringComparison.OrdinalIgnoreCase))
            return false;

        return name.Any(char.IsLetterOrDigit);
    }

    private int ReadTeam(nint controller, nint pawn)
    {
        if (pawn != nint.Zero)
        {
            var pawnTeam = _memory.Read<int>(pawn + _offsets.M_iTeamNum);
            if (pawnTeam is GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist)
                return pawnTeam;
        }

        if (_offsets.M_iPendingTeamNum != nint.Zero)
        {
            var pendingTeam = _memory.Read<int>(controller + _offsets.M_iPendingTeamNum);
            if (pendingTeam is GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist)
                return pendingTeam;
        }

        var controllerTeam = _memory.Read<int>(controller + _offsets.M_iTeamNum);
        if (controllerTeam is GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist)
            return controllerTeam;

        return 0;
    }

    private (bool IsAlive, int Health) ResolveAliveState(nint controller, nint pawn)
    {
        // Pawn health is authoritative for enemies — controller pawn fields are teammate-replicated only.
        if (pawn != nint.Zero)
        {
            var health = _memory.Read<int>(pawn + _offsets.M_iHealth);
            if (health <= 0)
                return (false, health);

            var lifeState = _memory.Read<byte>(pawn + _offsets.M_lifeState);
            var isAlive = lifeState != LifeStateDead;
            return (isAlive, health);
        }

        var controllerHealth = _memory.Read<int>(controller + _offsets.M_iPawnHealth);
        if (controllerHealth <= 0)
            return (false, controllerHealth);

        var controllerAlive = _memory.Read<byte>(controller + _offsets.M_bPawnIsAlive) != 0;
        return (controllerAlive, controllerHealth);
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

    private string ReadPlayerName(nint controller, int index)
    {
        if (_offsets.M_sSanitizedPlayerName != nint.Zero)
        {
            var namePointer = _memory.ReadPtr(controller + _offsets.M_sSanitizedPlayerName);
            var sanitized = _memory.ReadString(namePointer);
            if (!string.IsNullOrWhiteSpace(sanitized))
                return sanitized;
        }

        if (_offsets.M_iszPlayerName != nint.Zero)
        {
            var name = _memory.ReadString(controller + _offsets.M_iszPlayerName);
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return $"Player {index}";
    }

    private bool IsCurrentlyInMatch(nint clientBase, nint localPawn, int localTeam)
    {
        if (localPawn == nint.Zero || localTeam is not (GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist))
            return false;

        var gameRules = _memory.ReadPtr(clientBase + _offsets.DwGameRules);
        if (gameRules == nint.Zero)
            return true;

        return _memory.Read<byte>(gameRules + _offsets.M_bHasMatchStarted) != 0;
    }

    private int ResolveTeam(nint localPawn, nint localController, nint entityList)
    {
        if (localController != nint.Zero)
        {
            var pawnHandle = _memory.Read<uint>(localController + _offsets.M_hPlayerPawn);
            nint pawn = nint.Zero;
            if (pawnHandle is not (0 or 0xFFFFFFFF))
                pawn = ResolvePawnFromHandle(entityList, pawnHandle);

            var team = ReadTeam(localController, pawn);
            if (team is GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist)
                return team;
        }

        if (localPawn != nint.Zero)
        {
            var team = _memory.Read<int>(localPawn + _offsets.M_iTeamNum);
            if (team is GameOffsets.TeamTerrorist or GameOffsets.TeamCounterTerrorist)
                return team;
        }

        return 0;
    }

    private LegacyRoundInfo ReadLegacyRoundInfo(nint clientBase)
    {
        var gameRules = _memory.ReadPtr(clientBase + _offsets.DwGameRules);
        if (gameRules == nint.Zero)
            return LegacyRoundInfo.Empty;

        return new LegacyRoundInfo
        {
            TotalRoundsPlayed = _memory.Read<int>(gameRules + _offsets.M_totalRoundsPlayed),
            RoundStartCount = _memory.Read<int>(gameRules + _offsets.M_nRoundStartCount),
            RoundEndCount = _memory.Read<int>(gameRules + _offsets.M_nRoundEndCount),
            IsFreezePeriod = _memory.Read<byte>(gameRules + _offsets.M_bFreezePeriod) != 0,
            IsWarmupPeriod = _memory.Read<byte>(gameRules + _offsets.M_bWarmupPeriod) != 0,
            GamePhase = _memory.Read<int>(gameRules + _offsets.M_gamePhase),
            RoundWinStatus = _memory.Read<int>(gameRules + _offsets.M_iRoundWinStatus),
            RoundWinnerTeam = _memory.Read<int>(gameRules + _offsets.M_iRoundEndWinnerTeam)
        };
    }

    private static LegacyMemoryState UnmatchedState(
        nint localPawn,
        nint localController,
        int localTeam = 0,
        LegacyRoundInfo? round = null)
    {
        return new LegacyMemoryState
        {
            IsAttached = true,
            IsInGame = localPawn != nint.Zero,
            IsInMatch = false,
            LocalTeam = localTeam,
            Players = Array.Empty<LegacyPlayerInfo>(),
            Round = round ?? LegacyRoundInfo.Empty
        };
    }

    private nint ResolveControllerFromIndex(nint entityList, int index, int spacing)
    {
        var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((index & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return _memory.ReadPtr(listEntry + (nint)(spacing * (index & 0x1FF)));
    }

    private nint ResolvePawnFromHandle(nint entityList, uint pawnHandle, int spacing)
    {
        var listEntry = _memory.ReadPtr(entityList + (nint)(0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10));
        if (listEntry == nint.Zero)
            return nint.Zero;

        return _memory.ReadPtr(listEntry + (nint)(spacing * (pawnHandle & 0x1FF)));
    }

    private Vector3? ReadPawnWorldPosition(nint pawn)
    {
        if (pawn == nint.Zero || _offsets.M_vOldOrigin == nint.Zero)
            return null;

        var x = _memory.Read<float>(pawn + _offsets.M_vOldOrigin);
        var y = _memory.Read<float>(pawn + _offsets.M_vOldOrigin + 4);
        var z = _memory.Read<float>(pawn + _offsets.M_vOldOrigin + 8);
        var position = new Vector3(x, y, z);
        return position.IsValid ? position : null;
    }
}
