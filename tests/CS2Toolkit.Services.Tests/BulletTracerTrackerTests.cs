using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services.Tests;

public sealed class BulletTracerTrackerTests
{
    [Fact]
    public void Update_adds_tracers_and_prunes_by_kind_and_distance()
    {
        var tracker = new BulletTracerTracker();
        var options = new BulletTracerVisualOptions
        {
            Enabled = true,
            ShowLocal = true,
            ShowTeammates = false,
            ShowEnemies = true,
            DurationMs = 1000,
            MaxDistanceUnits = 5000f,
            MaxActiveTracers = 8
        };

        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            bulletImpacts:
            [
                new BulletImpactEvent(
                    local.Id,
                    BulletTracerKind.Local,
                    new Vector3(0f, 0f, 64f),
                    new Vector3(100f, 0f, 64f),
                    DateTimeOffset.UtcNow),
                new BulletImpactEvent(
                    new PlayerId(2),
                    BulletTracerKind.Teammate,
                    new Vector3(0f, 0f, 64f),
                    new Vector3(200f, 0f, 64f),
                    DateTimeOffset.UtcNow),
                new BulletImpactEvent(
                    new PlayerId(3),
                    BulletTracerKind.Enemy,
                    new Vector3(0f, 0f, 64f),
                    new Vector3(300f, 0f, 64f),
                    DateTimeOffset.UtcNow)
            ]);

        tracker.Update(snapshot, options);

        var state = tracker.CopyState();
        Assert.Equal(2, state.Tracers.Count);
        Assert.Contains(state.Tracers, tracer => tracer.Kind == BulletTracerKind.Local);
        Assert.Contains(state.Tracers, tracer => tracer.Kind == BulletTracerKind.Enemy);
        Assert.DoesNotContain(state.Tracers, tracer => tracer.Kind == BulletTracerKind.Teammate);
    }

    [Fact]
    public void Update_clears_when_disabled()
    {
        var tracker = new BulletTracerTracker();
        var local = GameSnapshotTestSupport.CreateLocalPlayer();
        var snapshot = GameSnapshotTestSupport.CreateInMatch(
            [GameSnapshotTestSupport.LocalPlayerEntity(local)],
            local,
            bulletImpacts:
            [
                new BulletImpactEvent(
                    local.Id,
                    BulletTracerKind.Local,
                    new Vector3(0f, 0f, 64f),
                    new Vector3(100f, 0f, 64f),
                    DateTimeOffset.UtcNow)
            ]);

        tracker.Update(snapshot, new BulletTracerVisualOptions { Enabled = true });
        Assert.Single(tracker.CopyState().Tracers);

        tracker.Update(snapshot, new BulletTracerVisualOptions { Enabled = false });
        Assert.Empty(tracker.CopyState().Tracers);
    }
}
