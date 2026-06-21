using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

public sealed class BulletTracerTracker
{
    private readonly object _lock = new();
    private readonly List<ActiveBulletTracer> _tracers = [];

    public BulletTracerState CopyState()
    {
        lock (_lock)
            return new BulletTracerState(_tracers.ToList());
    }

    public void Update(GameSnapshot snapshot, BulletTracerVisualOptions options)
    {
        if (!options.Enabled)
        {
            Reset();
            return;
        }

        if (!snapshot.IsAttached || !snapshot.IsInMatch)
        {
            Reset();
            return;
        }

        var now = DateTimeOffset.UtcNow;

        lock (_lock)
        {
            for (var i = _tracers.Count - 1; i >= 0; i--)
            {
                if ((now - _tracers[i].StartedAt).TotalMilliseconds >= options.DurationMs)
                    _tracers.RemoveAt(i);
            }

            foreach (var impact in snapshot.RecentBulletImpacts)
            {
                if (!ShouldShow(impact.Kind, options))
                    continue;

                if (!IsWithinDistance(snapshot, impact.Start, options.MaxDistanceUnits))
                    continue;

                _tracers.Add(new ActiveBulletTracer(
                    impact.Start,
                    impact.End,
                    impact.Kind,
                    impact.Timestamp));
            }

            var maxActive = Math.Max(1, options.MaxActiveTracers);
            while (_tracers.Count > maxActive)
                _tracers.RemoveAt(0);
        }
    }

    private static bool ShouldShow(BulletTracerKind kind, BulletTracerVisualOptions options) =>
        kind switch
        {
            BulletTracerKind.Local => options.ShowLocal,
            BulletTracerKind.Teammate => options.ShowTeammates,
            BulletTracerKind.Enemy => options.ShowEnemies,
            _ => false
        };

    private static bool IsWithinDistance(GameSnapshot snapshot, Vector3 position, float maxDistanceUnits)
    {
        var localPlayer = snapshot.Players.FirstOrDefault(player => player.IsLocalPlayer);
        if (localPlayer?.WorldPosition is not { } localPosition)
            return true;

        return localPosition.DistanceTo(position) <= maxDistanceUnits;
    }

    private void Reset()
    {
        lock (_lock)
            _tracers.Clear();
    }
}

public sealed record BulletTracerState(IReadOnlyList<ActiveBulletTracer> Tracers);

public sealed record ActiveBulletTracer(
    Vector3 Start,
    Vector3 End,
    BulletTracerKind Kind,
    DateTimeOffset StartedAt);
