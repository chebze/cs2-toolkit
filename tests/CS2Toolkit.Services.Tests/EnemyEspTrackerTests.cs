using CS2Toolkit.Models.Abstractions;
using CS2Toolkit.Services;

namespace CS2Toolkit.Services.Tests;

public sealed class EnemyEspTrackerTests
{
    [Fact]
    public void Update_when_detached_clears_targets()
    {
        var tracker = new EnemyEspTracker();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var players = new[]
        {
            GameSnapshotTestSupport.LocalPlayerEntity(local),
            GameSnapshotTestSupport.Enemy(2)
        };
        var inMatch = GameSnapshotTestSupport.CreateInMatch(players, local);

        tracker.Update(inMatch, EnemyEspMode.Full);
        Assert.Single(tracker.CopyDrawableTargets(EnemyEspMode.Full));

        tracker.Update(GameSnapshot.Detached, EnemyEspMode.Full);
        Assert.Empty(tracker.CopyDrawableTargets(EnemyEspMode.Full));
        Assert.Empty(tracker.CopyDrawableTargets(EnemyEspMode.LastSeen));
    }

    [Fact]
    public void Update_full_mode_tracks_live_enemies()
    {
        var tracker = new EnemyEspTracker();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var players = new[]
        {
            GameSnapshotTestSupport.LocalPlayerEntity(local),
            GameSnapshotTestSupport.Enemy(2),
            GameSnapshotTestSupport.Enemy(3)
        };

        tracker.Update(GameSnapshotTestSupport.CreateInMatch(players, local), EnemyEspMode.Full);

        var targets = tracker.CopyDrawableTargets(EnemyEspMode.Full);
        Assert.Equal(2, targets.Count);
        Assert.Contains(targets, target => target.PlayerId.Value == 2);
        Assert.Contains(targets, target => target.PlayerId.Value == 3);
    }

    [Fact]
    public void Update_last_seen_mode_captures_spotted_enemy_not_currently_visible()
    {
        var tracker = new EnemyEspTracker();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var players = new[]
        {
            GameSnapshotTestSupport.LocalPlayerEntity(local),
            GameSnapshotTestSupport.Enemy(2, spottedByTeam: true, visibleToLocal: false)
        };

        tracker.Update(GameSnapshotTestSupport.CreateInMatch(players, local), EnemyEspMode.LastSeen);

        var targets = tracker.CopyDrawableTargets(EnemyEspMode.LastSeen);
        Assert.Single(targets);
        Assert.Equal(2, targets[0].PlayerId.Value);
    }

    [Fact]
    public void Update_last_seen_mode_hides_currently_visible_enemy_from_drawable_list()
    {
        var tracker = new EnemyEspTracker();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var hiddenEnemy = GameSnapshotTestSupport.Enemy(2, spottedByTeam: true, visibleToLocal: false);
        var players = new[]
        {
            GameSnapshotTestSupport.LocalPlayerEntity(local),
            hiddenEnemy
        };
        var snapshot = GameSnapshotTestSupport.CreateInMatch(players, local);

        tracker.Update(snapshot, EnemyEspMode.LastSeen);
        Assert.Single(tracker.CopyDrawableTargets(EnemyEspMode.LastSeen));

        var visibleEnemy = GameSnapshotTestSupport.Enemy(2, spottedByTeam: true, visibleToLocal: true);
        tracker.Update(
            GameSnapshotTestSupport.CreateInMatch(
                [GameSnapshotTestSupport.LocalPlayerEntity(local), visibleEnemy],
                local),
            EnemyEspMode.LastSeen);

        Assert.Empty(tracker.CopyDrawableTargets(EnemyEspMode.LastSeen));
    }

    [Fact]
    public void Update_new_round_clears_previous_last_seen_targets()
    {
        var tracker = new EnemyEspTracker();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var players = new[]
        {
            GameSnapshotTestSupport.LocalPlayerEntity(local),
            GameSnapshotTestSupport.Enemy(2, spottedByTeam: true, visibleToLocal: false)
        };

        tracker.Update(
            GameSnapshotTestSupport.CreateInMatch(players, local, round: new RoundState(1, 1, 0, false, false, 0, 0, 0)),
            EnemyEspMode.LastSeen);
        Assert.Single(tracker.CopyDrawableTargets(EnemyEspMode.LastSeen));

        var roundTwoPlayers = new[]
        {
            GameSnapshotTestSupport.LocalPlayerEntity(local),
            GameSnapshotTestSupport.Enemy(2, spottedByTeam: false, visibleToLocal: false)
        };
        tracker.Update(
            GameSnapshotTestSupport.CreateInMatch(
                roundTwoPlayers,
                local,
                round: new RoundState(2, 2, 0, false, false, 0, 0, 0)),
            EnemyEspMode.LastSeen);

        Assert.Empty(tracker.CopyDrawableTargets(EnemyEspMode.LastSeen));
    }
}
