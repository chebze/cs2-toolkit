using Cs2Toolkit.Configuration;
using Cs2Toolkit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Memory;

public sealed class EnemyLastSeenTracker
{
    private const byte LifeStateDead = 2;

    private readonly ProcessMemory _memory;
    private readonly ILogger<EnemyLastSeenTracker> _logger;
    private readonly SkeletonOverlayOptions _skeletonOptions;
    private readonly Dictionary<int, EnemyLastSeenSnapshot> _lastSeen = new();
    private readonly HashSet<int> _visibleToLocalPlayer = new();
    private readonly Dictionary<int, DateTime> _lastReadLogAt = new();
    private readonly float[] _viewMatrix = new float[16];
    private readonly Vector3[] _boneScratch = new Vector3[PlayerBones.Count];

    private GameOffsets? _offsets;
    private int _trackedRoundStart = -1;

    public EnemyLastSeenTracker(
        ProcessMemory memory,
        IOptions<ToolkitOptions> options,
        ILogger<EnemyLastSeenTracker> logger)
    {
        _memory = memory;
        _logger = logger;
        _skeletonOptions = options.Value.Overlay.EnemyLastSeen;
    }

    public ReadOnlySpan<float> LatestViewMatrix => _viewMatrix;

    public IEnumerable<EnemyLastSeenSnapshot> DrawableSnapshots =>
        _lastSeen.Values.Where(snapshot => !_visibleToLocalPlayer.Contains(snapshot.PlayerIndex));

    public void Initialize(GameOffsets offsets) => _offsets = offsets;

    public void Poll(MemoryState state)
    {
        if (_offsets is null || !_memory.IsAttached || !state.IsInMatch || state.LocalTeam == 0)
        {
            ResetAll();
            return;
        }

        if (state.Round.RoundStartCount != _trackedRoundStart)
        {
            _lastSeen.Clear();
            _trackedRoundStart = state.Round.RoundStartCount;
        }

        ReadViewMatrix();

        var clientBase = _memory.ClientBase;
        var entityList = _memory.ReadPtr(clientBase + _offsets.DwEntityList);
        if (entityList == nint.Zero)
            return;

        var localPlayerIndex = ResolveLocalPlayerIndex(state, entityList);
        var friendlyIndices = new HashSet<int>();
        foreach (var player in state.Players)
        {
            if (player.Team == state.LocalTeam)
                friendlyIndices.Add(player.Index);
        }

        _visibleToLocalPlayer.Clear();

        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer || player.Team == state.LocalTeam)
                continue;

            if (!player.IsAlive)
            {
                _lastSeen.Remove(player.Index);
                continue;
            }

            var pawn = ResolvePawnForPlayer(entityList, player.Index);
            if (pawn == nint.Zero)
                continue;

            if (!IsPawnAlive(pawn))
            {
                _lastSeen.Remove(player.Index);
                continue;
            }

            if (IsSpottedByPlayer(pawn, localPlayerIndex))
                _visibleToLocalPlayer.Add(player.Index);

            if (!IsSpottedByFriendlyTeam(pawn, friendlyIndices))
                continue;

            if (!BoneHelper.TryReadSkeleton(_memory, _offsets, pawn, _boneScratch, out var boneContext))
                continue;

            var snapshot = new EnemyLastSeenSnapshot
            {
                PlayerIndex = player.Index,
                Name = player.Name,
                Bones = (Vector3[])_boneScratch.Clone(),
                LastSeenAt = DateTime.UtcNow
            };
            _lastSeen[player.Index] = snapshot;
            LogSkeletonRead(snapshot, boneContext, _visibleToLocalPlayer.Contains(player.Index));
        }
    }

    private void LogSkeletonRead(EnemyLastSeenSnapshot snapshot, BoneReadContext context, bool hiddenFromLocalPlayer)
    {
        if (!_skeletonOptions.LogDiagnostics)
            return;

        var now = DateTime.UtcNow;
        if (_lastReadLogAt.TryGetValue(snapshot.PlayerIndex, out var lastLogged)
            && (now - lastLogged).TotalMilliseconds < _skeletonOptions.LogDiagnosticsIntervalMs)
            return;

        _lastReadLogAt[snapshot.PlayerIndex] = now;
        _logger.LogInformation("{Diagnostics}", SkeletonDiagnostics.FormatRead(snapshot, context, hiddenFromLocalPlayer));
    }

    private bool IsPawnAlive(nint pawn)
    {
        var health = _memory.Read<int>(pawn + _offsets!.M_iHealth);
        if (health <= 0)
            return false;

        var lifeState = _memory.Read<byte>(pawn + _offsets.M_lifeState);
        return lifeState != LifeStateDead;
    }

    private int ResolveLocalPlayerIndex(MemoryState state, nint entityList)
    {
        foreach (var player in state.Players)
        {
            if (player.IsLocalPlayer)
                return player.Index;
        }

        var localController = _memory.ReadPtr(_memory.ClientBase + _offsets!.DwLocalPlayerController);
        if (localController == nint.Zero)
            return -1;

        for (var index = 1; index <= GameOffsets.MaxPlayerIndex; index++)
        {
            var controller = ResolveControllerFromIndex(entityList, index);
            if (controller == localController)
                return index;
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

    private void ReadViewMatrix()
    {
        var matrixAddress = _memory.ClientBase + _offsets!.DwViewMatrix;
        for (var i = 0; i < 16; i++)
            _viewMatrix[i] = _memory.Read<float>(matrixAddress + (nint)(i * 4));
    }

    private void ResetAll()
    {
        _lastSeen.Clear();
        _visibleToLocalPlayer.Clear();
        _trackedRoundStart = -1;
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
