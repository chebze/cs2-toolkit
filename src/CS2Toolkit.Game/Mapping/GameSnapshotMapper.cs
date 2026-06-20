using CS2Toolkit.Game.Internal;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Game.Mapping;

internal static class GameSnapshotMapper
{
    public static GameSnapshot Map(
        LegacyMemoryState state,
        string? mapName,
        ViewMatrix viewMatrix,
        LocalPlayer? localPlayer,
        GrenadeState grenade,
        TriggerbotState triggerbot,
        RcsState rcs,
        AimHelperState aimHelper,
        RadarSnapshot radar)
    {
        if (!state.IsAttached)
            return GameSnapshot.Detached;

        return new GameSnapshot(
            DateTimeOffset.UtcNow,
            state.IsAttached,
            state.IsInGame,
            state.IsInMatch,
            mapName,
            localPlayer,
            state.Players.Select(MapPlayer).ToArray(),
            MapRound(state.Round),
            MapBomb(state.Bomb),
            MapBombSites(state.BombSites),
            viewMatrix,
            state.RecentSounds,
            grenade.IsActive ? [grenade] : [],
            state.ClairvoyanceTips,
            state.EnemiesAlive,
            state.EnemiesDead,
            state.TeammatesAlive,
            state.TeammatesDead,
            triggerbot,
            rcs,
            aimHelper,
            radar);
    }

    private static Player MapPlayer(LegacyPlayerInfo player) => new(
        new PlayerId(player.Index),
        player.Name,
        MapTeam(player.Team),
        player.Health,
        player.IsAlive,
        player.IsLocalPlayer,
        player.WorldPosition,
        player.Bones,
        player.IsSpottedByTeam,
        player.IsVisibleToLocalPlayer);

    private static Team MapTeam(int team) => team switch
    {
        GameOffsets.TeamTerrorist => Team.Terrorist,
        GameOffsets.TeamCounterTerrorist => Team.CounterTerrorist,
        _ => Team.None
    };

    private static RoundState MapRound(LegacyRoundInfo round) => new(
        round.TotalRoundsPlayed,
        round.RoundStartCount,
        round.RoundEndCount,
        round.IsFreezePeriod,
        round.IsWarmupPeriod,
        round.GamePhase,
        round.RoundWinStatus,
        round.RoundWinnerTeam);

    private static BombState MapBomb(LegacyBombInfo bomb) => new(
        MapBombStatus(bomb.Status),
        bomb.Site,
        bomb.TimeLeftSeconds,
        bomb.HasDefuseKit,
        bomb.DefuseTimeSeconds,
        bomb.WillDefuseSucceed,
        bomb.WorldPosition is null ? null : MapVector(bomb.WorldPosition.Value));

    private static BombSitesInfo MapBombSites(LegacyBombSitesInfo sites) => new(
        MapVector(sites.CenterA),
        MapVector(sites.CenterB));

    private static BombStatus MapBombStatus(LegacyBombStatus status) => status switch
    {
        LegacyBombStatus.Carried => BombStatus.Carried,
        LegacyBombStatus.Equipped => BombStatus.Equipped,
        LegacyBombStatus.OnGround => BombStatus.OnGround,
        LegacyBombStatus.Defusing => BombStatus.Defusing,
        LegacyBombStatus.Planting => BombStatus.Planting,
        LegacyBombStatus.Planted => BombStatus.Planted,
        _ => BombStatus.None
    };

    private static Vector3 MapVector(LegacyVector3 vector) => new(vector.X, vector.Y, vector.Z);
}
