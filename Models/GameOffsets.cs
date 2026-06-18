namespace Cs2Toolkit.Models;

public sealed class GameOffsets
{
    public nint DwEntityList { get; init; }
    public nint DwLocalPlayerPawn { get; init; }
    public nint DwLocalPlayerController { get; init; }
    public nint DwGameRules { get; init; }
    public nint DwPlantedC4 { get; init; }
    public nint DwGlobalVars { get; init; }
    public nint DwWeaponC4 { get; init; }
    public nint DwViewMatrix { get; init; }
    public nint DwGameEntitySystemHighestEntityIndex { get; init; }
    public nint M_hPlayerPawn { get; init; }
    public nint M_iTeamNum { get; init; }
    public nint M_iHealth { get; init; }
    public nint M_lifeState { get; init; }
    public nint M_iszPlayerName { get; init; }
    public nint M_sSanitizedPlayerName { get; init; }
    public nint M_bHasMatchStarted { get; init; }
    public nint M_bPawnIsAlive { get; init; }
    public nint M_iPawnHealth { get; init; }
    public nint M_iConnected { get; init; }
    public nint M_iPendingTeamNum { get; init; }
    public nint M_bIsLocalPlayerController { get; init; }
    public nint M_iMatchStatsPlayersAliveCt { get; init; }
    public nint M_iMatchStatsPlayersAliveT { get; init; }
    public nint M_totalRoundsPlayed { get; init; }
    public nint M_nRoundStartCount { get; init; }
    public nint M_nRoundEndCount { get; init; }
    public nint M_bFreezePeriod { get; init; }
    public nint M_bWarmupPeriod { get; init; }
    public nint M_gamePhase { get; init; }
    public nint M_iRoundWinStatus { get; init; }
    public nint M_iRoundEndWinnerTeam { get; init; }
    public nint M_pWeaponServices { get; init; }
    public nint M_hActiveWeapon { get; init; }
    public nint M_hMyWeapons { get; init; }
    public nint M_AttributeManager { get; init; }
    public nint M_Item { get; init; }
    public nint M_iItemDefinitionIndex { get; init; }
    public nint M_bBombDropped { get; init; }
    public nint M_bBombPlanted { get; init; }
    public nint M_nBombSite { get; init; }
    public nint M_flC4Blow { get; init; }
    public nint M_flTimerLength { get; init; }
    public nint M_bBeingDefused { get; init; }
    public nint M_flDefuseCountDown { get; init; }
    public nint M_bCannotBeDefused { get; init; }
    public nint M_hBombDefuser { get; init; }
    public nint M_bStartedArming { get; init; }
    public nint M_bIsPlantingViaUse { get; init; }
    public nint M_nWhichBombZone { get; init; }
    public nint M_bIsDefusing { get; init; }
    public nint M_iBlockingUseActionInProgress { get; init; }
    public nint M_bPawnHasDefuser { get; init; }
    public nint M_pItemServices { get; init; }
    public nint M_bHasDefuser { get; init; }
    public nint M_vOldOrigin { get; init; }
    public nint M_iClip1 { get; init; }
    public nint M_bombsiteCenterA { get; init; }
    public nint M_bombsiteCenterB { get; init; }
    public nint M_pGameSceneNode { get; init; }
    public nint M_vecAbsOrigin { get; init; }
    public nint M_flDefuseLength { get; init; }
    public nint M_fLastDefuseTime { get; init; }
    public nint M_iProgressBarDuration { get; init; }
    public nint M_flProgressBarStartTime { get; init; }
    public nint M_bInBombZone { get; init; }
    public nint M_foundGoalPositions { get; init; }
    public nint M_flEmitSoundTime { get; init; }
    public nint M_bIsWalking { get; init; }
    public nint M_bInReload { get; init; }
    public nint M_pMovementServices { get; init; }
    public nint M_nLastJumpTick { get; init; }
    public nint M_entitySpottedState { get; init; }
    public nint M_modelState { get; init; }
    public nint M_pAimPunchServices { get; init; }
    public nint M_aimPunchCache { get; init; }
    public nint M_iShotsFired { get; init; }
    public nint M_bIsScoped { get; init; }
    public nint M_iIDEntIndex { get; init; }
    public nint M_angEyeAngles { get; init; }
    public nint M_vecViewOffset { get; init; }
    public nint M_vecAbsVelocity { get; init; }
    public nint DwGameTypes { get; init; }
    public nint M_bPinPulled { get; init; }
    public nint M_flThrowStrength { get; init; }
    public nint M_bIsHeldByPlayer { get; init; }
    public nint M_bGrenadeParametersStashed { get; init; }
    public nint M_angStashedShootAngles { get; init; }
    public nint M_vecStashedGrenadeThrowPosition { get; init; }
    public nint M_vecStashedVelocity { get; init; }
    public nint M_arrTrajectoryTrailPoints { get; init; }

    public const nint DwGameTypes_mapName = 0x120;
    public const nint EntitySpottedState_bSpotted = 0x8;
    public const nint EntitySpottedState_bSpottedByMask = 0xC;
    public const nint SceneNode_bDormant = 0x103;
    public const nint ModelStateBoneArray = 0x80;
    public const nint GlobalVarsCurtime = 0x30;
    public const nint GlobalVarsCurtimeAlt = 0x2C;
    public static readonly nint[] GlobalVarsCurtimeCandidates = [0x30, 0x2C, 0x34];
    public const int BlockingUseActionDefuseDefault = 1;
    public const int BlockingUseActionDefuseWithKit = 2;

    public const ushort WeaponC4 = 49;
    public const ushort WeaponKnife = 42;
    public const ushort WeaponKnifeT = 59;

    public const ushort WeaponFlashbang = 43;
    public const ushort WeaponHeGrenade = 44;
    public const ushort WeaponSmokeGrenade = 45;
    public const ushort WeaponMolotov = 46;
    public const ushort WeaponDecoy = 47;
    public const ushort WeaponIncendiary = 48;

    public static bool IsGrenadeWeapon(ushort weaponId) => weaponId switch
    {
        WeaponFlashbang or WeaponHeGrenade or WeaponSmokeGrenade
            or WeaponMolotov or WeaponDecoy or WeaponIncendiary => true,
        _ => false
    };

    public static float GetGrenadeThrowVelocity(ushort weaponId) => weaponId switch
    {
        WeaponDecoy => 700f,
        WeaponMolotov or WeaponIncendiary => 780f,
        _ => 750f
    };

    public static readonly int[] EntitySpacings = [0x70, 0x78];

    public const int MaxPlayerIndex = 64;
    public const int TeamTerrorist = 2;
    public const int TeamCounterTerrorist = 3;
}
