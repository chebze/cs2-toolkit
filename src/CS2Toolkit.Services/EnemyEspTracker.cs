using CS2Toolkit.Models;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Services;

public sealed class EnemyEspTracker
{
    private readonly object _lock = new();
    private readonly Dictionary<int, EspTarget> _lastSeen = new();
    private readonly Dictionary<int, EspTarget> _live = new();
    private readonly HashSet<int> _visibleToLocalPlayer = new();
    private int _trackedRoundStart = -1;

    public IReadOnlyList<EspTarget> CopyDrawableTargets(EnemyEspMode mode)
    {
        lock (_lock)
        {
            return mode switch
            {
                EnemyEspMode.Full => _live.Values.ToList(),
                EnemyEspMode.LastSeen => _lastSeen.Values
                    .Where(target => !_visibleToLocalPlayer.Contains(target.PlayerId.Value))
                    .ToList(),
                _ => []
            };
        }
    }

    public void Update(GameSnapshot snapshot, EnemyEspMode mode)
    {
        if (!snapshot.IsAttached || !snapshot.IsInMatch || mode == EnemyEspMode.Disabled)
        {
            ResetAll();
            return;
        }

        var localPlayer = snapshot.Players.FirstOrDefault(player => player.IsLocalPlayer);
        if (localPlayer is null || localPlayer.Team == Team.None)
        {
            ResetAll();
            return;
        }

        if (snapshot.Round.RoundStartCount != _trackedRoundStart)
        {
            lock (_lock)
            {
                _lastSeen.Clear();
                _live.Clear();
                _trackedRoundStart = snapshot.Round.RoundStartCount;
            }
        }

        if (mode == EnemyEspMode.Full)
            PollLiveSkeletons(snapshot, localPlayer.Team);
        else
        {
            lock (_lock)
                _live.Clear();

            PollLastSeenSkeletons(snapshot, localPlayer);
        }
    }

    private void PollLastSeenSkeletons(GameSnapshot snapshot, Player localPlayer)
    {
        var visibleToLocal = new HashSet<int>();
        var updates = new Dictionary<int, EspTarget>();

        foreach (var player in snapshot.Players)
        {
            if (player.IsLocalPlayer || player.Team == localPlayer.Team)
                continue;

            if (!player.IsAlive)
            {
                lock (_lock)
                    _lastSeen.Remove(player.Id.Value);
                continue;
            }

            if (!player.IsSpottedByTeam)
                continue;

            if (player.IsVisibleToLocalPlayer)
            {
                visibleToLocal.Add(player.Id.Value);
                if (TryCaptureTarget(player, out var snapshotTarget))
                    updates[player.Id.Value] = snapshotTarget;
                continue;
            }

            lock (_lock)
            {
                if (_lastSeen.ContainsKey(player.Id.Value))
                    continue;
            }

            if (TryCaptureTarget(player, out var firstSnapshot))
                updates[player.Id.Value] = firstSnapshot;
        }

        lock (_lock)
        {
            _visibleToLocalPlayer.Clear();
            foreach (var index in visibleToLocal)
                _visibleToLocalPlayer.Add(index);

            foreach (var (index, target) in updates)
                _lastSeen[index] = target;
        }
    }

    private void PollLiveSkeletons(GameSnapshot snapshot, Team localTeam)
    {
        var updates = new Dictionary<int, EspTarget>();
        var aliveIndices = new HashSet<int>();

        foreach (var player in snapshot.Players)
        {
            if (player.IsLocalPlayer || player.Team == localTeam)
                continue;

            if (!player.IsAlive)
                continue;

            aliveIndices.Add(player.Id.Value);

            if (TryCaptureTarget(player, out var target))
                updates[player.Id.Value] = target;
        }

        lock (_lock)
        {
            foreach (var index in _live.Keys.Where(index => !aliveIndices.Contains(index)).ToList())
                _live.Remove(index);

            foreach (var (index, target) in updates)
                _live[index] = target;

            _visibleToLocalPlayer.Clear();
            _lastSeen.Clear();
        }
    }

    private static bool TryCaptureTarget(Player player, out EspTarget target)
    {
        target = default!;

        if (player.Bones is not { } bones || !bones.HasValidSkeleton)
            return false;

        target = new EspTarget(
            player.Id,
            player.Name,
            player.Health,
            bones,
            DateTimeOffset.UtcNow);

        return true;
    }

    private void ResetAll()
    {
        lock (_lock)
        {
            _lastSeen.Clear();
            _live.Clear();
            _visibleToLocalPlayer.Clear();
            _trackedRoundStart = -1;
        }
    }
}
