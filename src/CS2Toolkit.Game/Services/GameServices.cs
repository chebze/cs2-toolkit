using System.Threading.Channels;
using CS2Toolkit.Configuration.Abstractions;
using CS2Toolkit.Game.Abstractions;
using CS2Toolkit.Game.Internal;
using CS2Toolkit.Game.Mapping;
using CS2Toolkit.Game.Maps;
using CS2Toolkit.Game.Memory;
using CS2Toolkit.Game.Offsets;
using CS2Toolkit.Game.Process;
using CS2Toolkit.Game.Readers;
using CS2Toolkit.Models.Abstractions;
using Microsoft.Extensions.Logging;

namespace CS2Toolkit.Game.Services;

public sealed class GameStatePublisher : IGameStateSource, IReadOnlyGameState
{
    private readonly Channel<GameSnapshot> _channel = Channel.CreateUnbounded<GameSnapshot>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = true
    });

    private GameSnapshot? _latest;

    public GameSnapshot? Latest => _latest;

    public async IAsyncEnumerable<GameSnapshot> WatchAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var snapshot in _channel.Reader.ReadAllAsync(cancellationToken))
            yield return snapshot;
    }

    internal void Publish(GameSnapshot snapshot)
    {
        _latest = snapshot;
        _channel.Writer.TryWrite(snapshot);
    }
}

internal sealed class GameSnapshotFactory
{
    private readonly ProcessMemory _memory;
    private readonly GameOffsets _offsets;
    private readonly EntitySnapshotReader _entityReader;
    private readonly MapNameReader _mapNameReader;
    private readonly ViewMatrixReader _viewMatrixReader;
    private readonly LocalPlayerReader _localPlayerReader;
    private readonly GrenadeTrajectoryReader _grenadeReader;
    private readonly TriggerbotReader _triggerbotReader;
    private readonly RcsReader _rcsReader;
    private readonly AimHelperReader _aimHelperReader;

    public GameSnapshotFactory(
        ProcessMemory memory,
        GameOffsets offsets,
        MapVisibilityChecker mapChecker,
        GrenadeSimulationOptions grenadeOptions)
    {
        _memory = memory;
        _offsets = offsets;
        _entityReader = new EntitySnapshotReader(memory, offsets, new ClairvoyanceOptionsStub());
        _mapNameReader = new MapNameReader();
        _viewMatrixReader = new ViewMatrixReader();
        _localPlayerReader = new LocalPlayerReader();
        _grenadeReader = new GrenadeTrajectoryReader(memory, offsets, mapChecker, grenadeOptions);
        _triggerbotReader = new TriggerbotReader(memory, offsets, mapChecker);
        _rcsReader = new RcsReader(memory, offsets);
        _aimHelperReader = new AimHelperReader(memory, offsets, mapChecker);
    }

    public GameSnapshot Create()
    {
        var legacy = _entityReader.ReadState();
        var mapName = _memory.IsAttached ? _mapNameReader.ReadCurrentMap(_memory, _offsets) : null;
        var viewMatrix = _viewMatrixReader.Read(_memory, _offsets);
        var localPlayer = _localPlayerReader.Read(_memory, _offsets, legacy);
        var grenade = _grenadeReader.Read(legacy.IsInMatch);
        var triggerbot = _triggerbotReader.Read(legacy);
        var rcs = _rcsReader.Read(legacy.IsInMatch);
        var viewAngles = localPlayer?.ViewAngles ?? default;
        var aimHelper = _aimHelperReader.Read(legacy, triggerbot.EyePosition, viewAngles);
        return GameSnapshotMapper.Map(legacy, mapName, viewMatrix, localPlayer, grenade, triggerbot, rcs, aimHelper);
    }
}

public sealed class GameAttachmentService : IGameAttachment, IGameLifecycle
{
    private readonly ProcessMemory _memory;
    private readonly ILogger<GameAttachmentService> _logger;

    public GameAttachmentService(ProcessMemory memory, ILogger<GameAttachmentService> logger)
    {
        _memory = memory;
        _logger = logger;
    }

    public bool IsAttached => _memory.IsAttached;
    public GameLifecycleState State { get; private set; } = GameLifecycleState.WaitingForOffsets;
    public event Action<GameLifecycleState>? StateChanged;

    public bool TryAttach(string processName = "cs2")
    {
        SetState(GameLifecycleState.WaitingForAttach);
        var attached = _memory.AttachToProcess(processName);
        if (!attached)
        {
            SetState(GameLifecycleState.Failed);
            _logger.LogWarning("Failed to attach to {ProcessName}", processName);
            return false;
        }

        SetState(GameLifecycleState.Attached);
        _logger.LogInformation("Attached to {ProcessName}", processName);
        return true;
    }

    public void Detach()
    {
        _memory.Detach();
        SetState(GameLifecycleState.WaitingForAttach);
        _logger.LogInformation("Detached from game process");
    }

    internal void SetState(GameLifecycleState state)
    {
        if (State == state)
            return;

        State = state;
        StateChanged?.Invoke(state);
    }
}

internal sealed class OffsetProviderService : IOffsetProvider
{
    private readonly OffsetDownloader _downloader;

    public OffsetProviderService(OffsetDownloader downloader) => _downloader = downloader;

    public OffsetMetadata Metadata { get; private set; } =
        new("not-loaded", DateTimeOffset.MinValue, false);

    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        await _downloader.DownloadAsync(cancellationToken);
        Metadata = new OffsetMetadata("remote", DateTimeOffset.UtcNow, _downloader.Offsets is not null);
    }

    internal GameOffsets RequireOffsets() =>
        _downloader.Offsets ?? throw new InvalidOperationException("Offsets are not loaded.");
}

internal sealed class MapCatalogService : IMapCatalog
{
    private readonly ProcessMemory _memory;
    private readonly OffsetDownloader _offsets;
    private readonly MapNameReader _mapNameReader = new();

    public MapCatalogService(ProcessMemory memory, OffsetDownloader offsets)
    {
        _memory = memory;
        _offsets = offsets;
    }

    public string? CurrentMap =>
        _memory.IsAttached && _offsets.Offsets is not null
            ? _mapNameReader.ReadCurrentMap(_memory, _offsets.Offsets)
            : null;
}
