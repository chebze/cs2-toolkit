using System.Text.Json;
using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Offsets;

public sealed class OffsetDownloader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ToolkitOptions _options;
    private readonly ILogger<OffsetDownloader> _logger;

    public GameOffsets? Offsets { get; private set; }

    public OffsetDownloader(
        IHttpClientFactory httpClientFactory,
        IOptions<ToolkitOptions> options,
        ILogger<OffsetDownloader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GameOffsets> DownloadAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading offsets from {OffsetsUrl}", _options.Offsets.OffsetsUrl);

        var httpClient = _httpClientFactory.CreateClient(nameof(OffsetDownloader));

        var offsetsJson = await httpClient.GetStringAsync(_options.Offsets.OffsetsUrl, cancellationToken);
        var clientDllJson = await httpClient.GetStringAsync(_options.Offsets.ClientDllUrl, cancellationToken);

        using var offsetsDoc = JsonDocument.Parse(offsetsJson);
        using var clientDoc = JsonDocument.Parse(clientDllJson);

        var clientOffsets = offsetsDoc.RootElement.GetProperty("client.dll");
        var matchmakingOffsets = offsetsDoc.RootElement.GetProperty("matchmaking.dll");

        var entityList = GetClientOffset(clientOffsets, "dwEntityList");
        var localPawn = GetClientOffset(clientOffsets, "dwLocalPlayerPawn");
        var localController = GetClientOffset(clientOffsets, "dwLocalPlayerController");
        var gameRules = GetClientOffset(clientOffsets, "dwGameRules");
        var plantedC4 = GetClientOffset(clientOffsets, "dwPlantedC4");
        var globalVars = GetClientOffset(clientOffsets, "dwGlobalVars");
        var weaponC4 = GetClientOffset(clientOffsets, "dwWeaponC4");
        var viewMatrix = GetClientOffset(clientOffsets, "dwViewMatrix");
        var highestEntityIndex = GetClientOffset(clientOffsets, "dwGameEntitySystem_highestEntityIndex");
        var dwGameTypes = GetClientOffset(matchmakingOffsets, "dwGameTypes");

        var clientClasses = clientDoc.RootElement.GetProperty("client.dll").GetProperty("classes");

        var m_hPlayerPawn = GetClassFieldOffset(clientClasses, "CCSPlayerController", "m_hPlayerPawn");
        var m_iTeamNum = GetClassFieldOffset(clientClasses, "C_BaseEntity", "m_iTeamNum");
        var m_iHealth = GetClassFieldOffset(clientClasses, "C_BaseEntity", "m_iHealth");
        var m_lifeState = GetClassFieldOffset(clientClasses, "C_BaseEntity", "m_lifeState");
        var m_bHasMatchStarted = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_bHasMatchStarted");
        var m_bPawnIsAlive = GetClassFieldOffset(clientClasses, "CCSPlayerController", "m_bPawnIsAlive");
        var m_iPawnHealth = GetClassFieldOffset(clientClasses, "CCSPlayerController", "m_iPawnHealth");
        var m_iConnected = GetClassFieldOffset(clientClasses, "CBasePlayerController", "m_iConnected");
        var m_iPendingTeamNum = GetClassFieldOffset(clientClasses, "CCSPlayerController", "m_iPendingTeamNum");
        var m_bIsLocalPlayerController = GetClassFieldOffset(clientClasses, "CBasePlayerController", "m_bIsLocalPlayerController");
        var m_iMatchStatsPlayersAliveCt = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_iMatchStats_PlayersAlive_CT");
        var m_iMatchStatsPlayersAliveT = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_iMatchStats_PlayersAlive_T");
        var m_totalRoundsPlayed = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_totalRoundsPlayed");
        var m_nRoundStartCount = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_nRoundStartCount");
        var m_nRoundEndCount = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_nRoundEndCount");
        var m_bFreezePeriod = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_bFreezePeriod");
        var m_bWarmupPeriod = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_bWarmupPeriod");
        var m_gamePhase = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_gamePhase");
        var m_iRoundWinStatus = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_iRoundWinStatus");
        var m_iRoundEndWinnerTeam = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_iRoundEndWinnerTeam");
        var m_pWeaponServices = GetClassFieldOffset(clientClasses, "C_BasePlayerPawn", "m_pWeaponServices");
        var m_hActiveWeapon = GetClassFieldOffset(clientClasses, "CPlayer_WeaponServices", "m_hActiveWeapon");
        var m_hMyWeapons = GetClassFieldOffset(clientClasses, "CPlayer_WeaponServices", "m_hMyWeapons");
        var m_AttributeManager = GetClassFieldOffset(clientClasses, "C_EconEntity", "m_AttributeManager");
        var m_Item = GetClassFieldOffset(clientClasses, "C_AttributeContainer", "m_Item");
        var m_iItemDefinitionIndex = GetClassFieldOffset(clientClasses, "C_EconItemView", "m_iItemDefinitionIndex");
        var m_bBombDropped = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_bBombDropped");
        var m_bBombPlanted = GetClassFieldOffset(clientClasses, "C_CSGameRules", "m_bBombPlanted");
        var m_nBombSite = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_nBombSite");
        var m_flC4Blow = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_flC4Blow");
        var m_flTimerLength = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_flTimerLength");
        var m_bBeingDefused = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_bBeingDefused");
        var m_flDefuseCountDown = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_flDefuseCountDown");
        var m_bCannotBeDefused = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_bCannotBeDefused");
        var m_hBombDefuser = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_hBombDefuser");
        var m_bStartedArming = GetClassFieldOffset(clientClasses, "C_C4", "m_bStartedArming");
        var m_bIsPlantingViaUse = GetClassFieldOffset(clientClasses, "C_C4", "m_bIsPlantingViaUse");
        var m_nWhichBombZone = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_nWhichBombZone");
        var m_bIsDefusing = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_bIsDefusing");
        var m_iBlockingUseActionInProgress = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_iBlockingUseActionInProgress");
        var m_bPawnHasDefuser = GetClassFieldOffset(clientClasses, "CCSPlayerController", "m_bPawnHasDefuser");
        var m_pItemServices = GetClassFieldOffset(clientClasses, "C_BasePlayerPawn", "m_pItemServices");
        var m_bHasDefuser = GetClassFieldOffset(clientClasses, "CCSPlayer_ItemServices", "m_bHasDefuser");
        var m_vOldOrigin = GetClassFieldOffset(clientClasses, "C_BasePlayerPawn", "m_vOldOrigin");
        var m_iClip1 = GetClassFieldOffset(clientClasses, "C_BasePlayerWeapon", "m_iClip1");
        var m_bombsiteCenterA = GetClassFieldOffset(clientClasses, "C_CSPlayerResource", "m_bombsiteCenterA");
        var m_bombsiteCenterB = GetClassFieldOffset(clientClasses, "C_CSPlayerResource", "m_bombsiteCenterB");
        var m_pGameSceneNode = GetClassFieldOffset(clientClasses, "C_BaseEntity", "m_pGameSceneNode");
        var m_vecAbsOrigin = GetClassFieldOffset(clientClasses, "CGameSceneNode", "m_vecAbsOrigin");
        var m_foundGoalPositions = GetClassFieldOffset(clientClasses, "C_CSPlayerResource", "m_foundGoalPositions");
        var m_flDefuseLength = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_flDefuseLength");
        var m_fLastDefuseTime = GetClassFieldOffset(clientClasses, "C_PlantedC4", "m_fLastDefuseTime");
        var m_iProgressBarDuration = GetClassFieldOffset(clientClasses, "C_CSPlayerPawnBase", "m_iProgressBarDuration");
        var m_flProgressBarStartTime = GetClassFieldOffset(clientClasses, "C_CSPlayerPawnBase", "m_flProgressBarStartTime");
        var m_bInBombZone = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_bInBombZone");
        var m_flEmitSoundTime = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_flEmitSoundTime");
        var m_bIsWalking = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_bIsWalking");
        var m_bInReload = TryGetClassFieldOffset(clientClasses, "m_bInReload", "C_CSWeaponBase", "C_BasePlayerWeapon")
            ?? nint.Zero;
        var m_pMovementServices = TryGetClassFieldOffset(clientClasses, "m_pMovementServices", "C_BasePlayerPawn", "C_CSPlayerPawn")
            ?? nint.Zero;
        var m_nLastJumpTick = TryGetClassFieldOffset(clientClasses, "m_nLastJumpTick", "CCSPlayer_MovementServices", "CPlayer_MovementServices")
            ?? nint.Zero;
        var m_entitySpottedState = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_entitySpottedState");
        var m_modelState = GetClassFieldOffset(clientClasses, "CSkeletonInstance", "m_modelState");
        var m_pAimPunchServices = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_pAimPunchServices");
        var m_iShotsFired = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_iShotsFired");
        var m_bIsScoped = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_bIsScoped");
        var m_iIDEntIndex = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_iIDEntIndex");
        var m_angEyeAngles = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_angEyeAngles");
        var m_vecViewOffset = GetClassFieldOffset(clientClasses, "C_BaseModelEntity", "m_vecViewOffset");
        var m_vecAbsVelocity = GetClassFieldOffset(clientClasses, "C_BaseEntity", "m_vecAbsVelocity");
        var m_bPinPulled = GetClassFieldOffset(clientClasses, "C_BaseCSGrenade", "m_bPinPulled");
        var m_flThrowStrength = GetClassFieldOffset(clientClasses, "C_BaseCSGrenade", "m_flThrowStrength");
        var m_bIsHeldByPlayer = GetClassFieldOffset(clientClasses, "C_BaseCSGrenade", "m_bIsHeldByPlayer");
        var m_bGrenadeParametersStashed = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_bGrenadeParametersStashed");
        var m_angStashedShootAngles = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_angStashedShootAngles");
        var m_vecStashedGrenadeThrowPosition = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_vecStashedGrenadeThrowPosition");
        var m_vecStashedVelocity = GetClassFieldOffset(clientClasses, "C_CSPlayerPawn", "m_vecStashedVelocity");
        var m_arrTrajectoryTrailPoints = GetClassFieldOffset(clientClasses, "C_BaseCSGrenadeProjectile", "m_arrTrajectoryTrailPoints");
        var m_unpredictableBaseTick = GetClassFieldOffset(clientClasses, "CCSPlayer_AimPunchServices", "m_unpredictableBaseTick");
        var m_aimPunchCache = m_unpredictableBaseTick - 0x18;
        var m_sSanitizedPlayerName = TryGetClassFieldOffset(clientClasses, "m_sSanitizedPlayerName", "CCSPlayerController")
            ?? nint.Zero;
        var m_iszPlayerName = TryGetClassFieldOffset(clientClasses, "m_iszPlayerName", "CBasePlayerController", "CCSPlayerController")
            ?? TryGetClassFieldOffset(clientClasses, "m_sSanitizedPlayerName", "CCSPlayerController")
            ?? nint.Zero;

        if (m_iszPlayerName == nint.Zero)
            _logger.LogWarning("Player name offset not found; names will use fallback labels");

        if (m_bInReload == nint.Zero)
            _logger.LogWarning("Reload offset not found; reload sounds will not be classified");

        if (m_pMovementServices == nint.Zero || m_nLastJumpTick == nint.Zero)
            _logger.LogWarning("Jump movement offsets not found; jump sounds will rely on emit time only");

        Offsets = new GameOffsets
        {
            DwEntityList = entityList,
            DwLocalPlayerPawn = localPawn,
            DwLocalPlayerController = localController,
            DwGameRules = gameRules,
            DwPlantedC4 = plantedC4,
            DwGlobalVars = globalVars,
            DwWeaponC4 = weaponC4,
            DwViewMatrix = viewMatrix,
            DwGameEntitySystemHighestEntityIndex = highestEntityIndex,
            DwGameTypes = dwGameTypes,
            M_hPlayerPawn = m_hPlayerPawn,
            M_iTeamNum = m_iTeamNum,
            M_iHealth = m_iHealth,
            M_lifeState = m_lifeState,
            M_iszPlayerName = m_iszPlayerName,
            M_sSanitizedPlayerName = m_sSanitizedPlayerName,
            M_bHasMatchStarted = m_bHasMatchStarted,
            M_bPawnIsAlive = m_bPawnIsAlive,
            M_iPawnHealth = m_iPawnHealth,
            M_iConnected = m_iConnected,
            M_iPendingTeamNum = m_iPendingTeamNum,
            M_bIsLocalPlayerController = m_bIsLocalPlayerController,
            M_iMatchStatsPlayersAliveCt = m_iMatchStatsPlayersAliveCt,
            M_iMatchStatsPlayersAliveT = m_iMatchStatsPlayersAliveT,
            M_totalRoundsPlayed = m_totalRoundsPlayed,
            M_nRoundStartCount = m_nRoundStartCount,
            M_nRoundEndCount = m_nRoundEndCount,
            M_bFreezePeriod = m_bFreezePeriod,
            M_bWarmupPeriod = m_bWarmupPeriod,
            M_gamePhase = m_gamePhase,
            M_iRoundWinStatus = m_iRoundWinStatus,
            M_iRoundEndWinnerTeam = m_iRoundEndWinnerTeam,
            M_pWeaponServices = m_pWeaponServices,
            M_hActiveWeapon = m_hActiveWeapon,
            M_hMyWeapons = m_hMyWeapons,
            M_AttributeManager = m_AttributeManager,
            M_Item = m_Item,
            M_iItemDefinitionIndex = m_iItemDefinitionIndex,
            M_bBombDropped = m_bBombDropped,
            M_bBombPlanted = m_bBombPlanted,
            M_nBombSite = m_nBombSite,
            M_flC4Blow = m_flC4Blow,
            M_flTimerLength = m_flTimerLength,
            M_bBeingDefused = m_bBeingDefused,
            M_flDefuseCountDown = m_flDefuseCountDown,
            M_bCannotBeDefused = m_bCannotBeDefused,
            M_hBombDefuser = m_hBombDefuser,
            M_bStartedArming = m_bStartedArming,
            M_bIsPlantingViaUse = m_bIsPlantingViaUse,
            M_nWhichBombZone = m_nWhichBombZone,
            M_bIsDefusing = m_bIsDefusing,
            M_iBlockingUseActionInProgress = m_iBlockingUseActionInProgress,
            M_bPawnHasDefuser = m_bPawnHasDefuser,
            M_pItemServices = m_pItemServices,
            M_bHasDefuser = m_bHasDefuser,
            M_vOldOrigin = m_vOldOrigin,
            M_iClip1 = m_iClip1,
            M_bombsiteCenterA = m_bombsiteCenterA,
            M_bombsiteCenterB = m_bombsiteCenterB,
            M_pGameSceneNode = m_pGameSceneNode,
            M_vecAbsOrigin = m_vecAbsOrigin,
            M_flDefuseLength = m_flDefuseLength,
            M_fLastDefuseTime = m_fLastDefuseTime,
            M_iProgressBarDuration = m_iProgressBarDuration,
            M_flProgressBarStartTime = m_flProgressBarStartTime,
            M_bInBombZone = m_bInBombZone,
            M_foundGoalPositions = m_foundGoalPositions,
            M_flEmitSoundTime = m_flEmitSoundTime,
            M_bIsWalking = m_bIsWalking,
            M_bInReload = m_bInReload,
            M_pMovementServices = m_pMovementServices,
            M_nLastJumpTick = m_nLastJumpTick,
            M_entitySpottedState = m_entitySpottedState,
            M_modelState = m_modelState,
            M_pAimPunchServices = m_pAimPunchServices,
            M_aimPunchCache = m_aimPunchCache,
            M_iShotsFired = m_iShotsFired,
            M_bIsScoped = m_bIsScoped,
            M_iIDEntIndex = m_iIDEntIndex,
            M_angEyeAngles = m_angEyeAngles,
            M_vecViewOffset = m_vecViewOffset,
            M_vecAbsVelocity = m_vecAbsVelocity,
            M_bPinPulled = m_bPinPulled,
            M_flThrowStrength = m_flThrowStrength,
            M_bIsHeldByPlayer = m_bIsHeldByPlayer,
            M_bGrenadeParametersStashed = m_bGrenadeParametersStashed,
            M_angStashedShootAngles = m_angStashedShootAngles,
            M_vecStashedGrenadeThrowPosition = m_vecStashedGrenadeThrowPosition,
            M_vecStashedVelocity = m_vecStashedVelocity,
            M_arrTrajectoryTrailPoints = m_arrTrajectoryTrailPoints
        };

        _logger.LogInformation("Offsets downloaded successfully");
        return Offsets;
    }

    private static nint GetClientOffset(JsonElement clientOffsets, string name)
    {
        if (!clientOffsets.TryGetProperty(name, out var value))
            throw new InvalidOperationException($"Missing client.dll offset: {name}");

        return (nint)value.GetInt64();
    }

    private static nint? TryGetClassFieldOffset(JsonElement classes, string fieldName, params string[] classNames)
    {
        foreach (var className in classNames)
        {
            if (!classes.TryGetProperty(className, out var classElement))
                continue;

            var fields = classElement.GetProperty("fields");
            if (!fields.TryGetProperty(fieldName, out var fieldValue))
                continue;

            return (nint)fieldValue.GetInt64();
        }

        return null;
    }

    private static nint GetClassFieldOffset(JsonElement classes, string className, string fieldName)
    {
        return TryGetClassFieldOffset(classes, fieldName, className)
            ?? throw new InvalidOperationException($"Missing field {className}.{fieldName}");
    }
}
