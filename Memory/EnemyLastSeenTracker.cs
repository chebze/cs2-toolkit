using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Memory;

public sealed class EnemyLastSeenTracker
{
    private const byte LifeStateDead = 2;

    private readonly ProcessMemory _memory;
    private readonly object _lock = new();
    private readonly Dictionary<int, EnemyLastSeenSnapshot> _lastSeen = new();
    private readonly Dictionary<int, EnemyLastSeenSnapshot> _live = new();
    private readonly HashSet<int> _visibleToLocalPlayer = new();
    private readonly Vector3[] _boneScratch = new Vector3[PlayerBones.Count];

    private GameOffsets? _offsets;
    private int _trackedRoundStart = -1;

    public EnemyLastSeenTracker(
        ProcessMemory memory,
        IOptions<ToolkitOptions> options)
    {
        _memory = memory;
        _ = options;
    }

    public List<EnemyLastSeenSnapshot> CopyDrawableSnapshots()
    {
        lock (_lock)
        {
            return _lastSeen.Values
                .Where(snapshot => !_visibleToLocalPlayer.Contains(snapshot.PlayerIndex))
                .ToList();
        }
    }

    public List<EnemyLastSeenSnapshot> CopyLiveSnapshots()
    {
        lock (_lock)
            return _live.Values.ToList();
    }

    public void Initialize(GameOffsets offsets) => _offsets = offsets;

    public void Poll(MemoryState state, EnemyEspMode mode)
    {
        if (_offsets is null || !_memory.IsAttached || !state.IsInMatch || state.LocalTeam == 0)
        {
            ResetAll();
            return;
        }

        if (mode == EnemyEspMode.Disabled)
        {
            ResetAll();
            return;
        }

        if (state.Round.RoundStartCount != _trackedRoundStart)
        {
            lock (_lock)
            {
                _lastSeen.Clear();
                _live.Clear();
                _trackedRoundStart = state.Round.RoundStartCount;
            }
        }

        if (mode == EnemyEspMode.Full)
            PollLiveSkeletons(state);
        else
        {
            lock (_lock)
                _live.Clear();

            PollLastSeenSkeletons(state);
        }
    }

    private void PollLastSeenSkeletons(MemoryState state)
    {
        var clientBase = _memory.ClientBase;
        var entityList = _memory.ReadPtr(clientBase + _offsets!.DwEntityList);
        if (entityList == nint.Zero)
            return;

        var localPlayerIndex = ResolveLocalPlayerIndex(state);
        var friendlyIndices = new HashSet<int>();
        foreach (var player in state.Players)
        {
            if (player.Team == state.LocalTeam)
                friendlyIndices.Add(player.Index);
        }

        var visibleToLocal = new HashSet<int>();
        var updates = new Dictionary<int, EnemyLastSeenSnapshot>();

        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || player.Team == state.LocalTeam)
                continue;

            if (!player.IsAlive)
            {
                lock (_lock)
                    _lastSeen.Remove(player.Index);
                continue;
            }

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (!IsPawnAlive(pawn))
            {
                lock (_lock)
                    _lastSeen.Remove(player.Index);
                continue;
            }

            var spottedByTeam = IsSpottedByFriendlyTeam(pawn, friendlyIndices);
            if (!spottedByTeam)
                continue;

            var visibleToLocalPlayer = IsSpottedByPlayer(pawn, localPlayerIndex);
            if (visibleToLocalPlayer)
            {
                visibleToLocal.Add(player.Index);
                if (TryCaptureSnapshot(player, pawn, out var snapshot))
                    updates[player.Index] = snapshot;
                continue;
            }

            lock (_lock)
            {
                if (_lastSeen.ContainsKey(player.Index))
                    continue;
            }

            if (TryCaptureSnapshot(player, pawn, out var firstSnapshot))
                updates[player.Index] = firstSnapshot;
        }

        lock (_lock)
        {
            _visibleToLocalPlayer.Clear();
            foreach (var index in visibleToLocal)
                _visibleToLocalPlayer.Add(index);

            foreach (var (index, snapshot) in updates)
                _lastSeen[index] = snapshot;
        }
    }

    private void PollLiveSkeletons(MemoryState state)
    {
        var clientBase = _memory.ClientBase;
        var entityList = _memory.ReadPtr(clientBase + _offsets!.DwEntityList);
        if (entityList == nint.Zero)
            return;

        var updates = new Dictionary<int, EnemyLastSeenSnapshot>();
        var aliveIndices = new HashSet<int>();

        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || player.Team == state.LocalTeam)
                continue;

            if (!player.IsAlive)
                continue;

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero || !IsPawnAlive(pawn))
                continue;

            aliveIndices.Add(player.Index);

            if (TryCaptureSnapshot(player, pawn, out var snapshot))
                updates[player.Index] = snapshot;
        }

        lock (_lock)
        {
            foreach (var index in _live.Keys.Where(index => !aliveIndices.Contains(index)).ToList())
                _live.Remove(index);

            foreach (var (index, snapshot) in updates)
                _live[index] = snapshot;

            _visibleToLocalPlayer.Clear();
            _lastSeen.Clear();
        }
    }

    private bool TryCaptureSnapshot(PlayerInfo player, nint pawn, out EnemyLastSeenSnapshot snapshot)
    {
        snapshot = default!;

        if (!BoneHelper.TryReadSkeleton(_memory, _offsets!, pawn, _boneScratch))
            return false;

        snapshot = new EnemyLastSeenSnapshot
        {
            PlayerIndex = player.Index,
            Name = player.Name,
            Bones = (Vector3[])_boneScratch.Clone(),
            LastSeenAt = DateTime.UtcNow
        };

        return snapshot.HasValidBones;
    }

    private bool IsPawnAlive(nint pawn)
    {
        var health = _memory.Read<int>(pawn + _offsets!.M_iHealth);
        if (health <= 0)
            return false;

        var lifeState = _memory.Read<byte>(pawn + _offsets.M_lifeState);
        return lifeState != LifeStateDead;
    }

    private int ResolveLocalPlayerIndex(MemoryState state)
    {
        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer)
                return player.Index;
        }

        return -1;
    }

    private bool IsSpottedByPlayer(nint pawn, int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 64)
            return false;

        var spottedState = pawn + _offsets!.M_entitySpottedState;
        var maskIndex = playerIndex / 32;
        var bit = playerIndex % 32;
        var mask = _memory.Read<uint>(spottedState + GameOffsets.EntitySpottedState_bSpottedByMask + (nint)(maskIndex * 4));
        return (mask & (1u << bit)) != 0;
    }

    private bool IsSpottedByFriendlyTeam(nint pawn, HashSet<int> friendlyIndices)
    {
        var spottedState = pawn + _offsets!.M_entitySpottedState;

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

    private nint ResolvePawnForPlayer(nint entityList, int index)
    {
        var controller = ResolveControllerFromIndex(entityList, index);
        if (controller == nint.Zero)
            return nint.Zero;

        var pawnHandle = _memory.Read<uint>(controller + _offsets!.M_hPlayerPawn);
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
}
