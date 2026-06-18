using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Models;
using Cs2Toolkit.Offsets;
using Cs2Toolkit.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class GameMemoryReader : BackgroundService
{
    private readonly ProcessMemory _processMemory;
    private readonly OffsetDownloader _offsetDownloader;
    private readonly RuntimeGate _runtimeGate;
    private readonly ToolkitEventBus _eventBus;
    private readonly EnemySoundTracker _enemySoundTracker;
    private readonly EnemyLastSeenTracker _enemyLastSeenTracker;
    private readonly RecoilCompensator _recoilCompensator;
    private readonly Triggerbot _triggerbot;
    private readonly RcsState _rcsState;
    private readonly TbState _tbState;
    private readonly MapDataService _mapDataService;
    private readonly MapNameReader _mapNameReader;
    private readonly GrenadeTrajectoryTracker _grenadeTrajectoryTracker;
    private readonly ToolkitOptions _options;
    private readonly ILogger<GameMemoryReader> _logger;

    private EntityResolver? _entityResolver;

    public GameMemoryReader(
        ProcessMemory processMemory,
        OffsetDownloader offsetDownloader,
        RuntimeGate runtimeGate,
        ToolkitEventBus eventBus,
        EnemySoundTracker enemySoundTracker,
        EnemyLastSeenTracker enemyLastSeenTracker,
        RecoilCompensator recoilCompensator,
        Triggerbot triggerbot,
        RcsState rcsState,
        TbState tbState,
        MapDataService mapDataService,
        MapNameReader mapNameReader,
        GrenadeTrajectoryTracker grenadeTrajectoryTracker,
        IOptions<ToolkitOptions> options,
        ILogger<GameMemoryReader> logger)
    {
        _processMemory = processMemory;
        _offsetDownloader = offsetDownloader;
        _runtimeGate = runtimeGate;
        _eventBus = eventBus;
        _enemySoundTracker = enemySoundTracker;
        _enemyLastSeenTracker = enemyLastSeenTracker;
        _recoilCompensator = recoilCompensator;
        _triggerbot = triggerbot;
        _rcsState = rcsState;
        _tbState = tbState;
        _mapDataService = mapDataService;
        _mapNameReader = mapNameReader;
        _grenadeTrajectoryTracker = grenadeTrajectoryTracker;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GameMemoryReader waiting for overlay and injection");

        await _runtimeGate.MemoryReaderStartTask.WaitAsync(stoppingToken);

        if (_offsetDownloader.Offsets is null)
            throw new InvalidOperationException("Offsets were not downloaded before memory reading started.");

        _entityResolver = new EntityResolver(_processMemory, _offsetDownloader.Offsets, _options.Clairvoyance);
        _enemySoundTracker.Initialize(_offsetDownloader.Offsets);
        _enemyLastSeenTracker.Initialize(_offsetDownloader.Offsets);
        _recoilCompensator.Initialize(_offsetDownloader.Offsets, _options.Rcs);
        _triggerbot.Initialize(_offsetDownloader.Offsets, _options.Tb, _mapDataService.VisibilityChecker);
        _grenadeTrajectoryTracker.Initialize(_offsetDownloader.Offsets, _mapDataService.VisibilityChecker);
        _logger.LogInformation("GameMemoryReader started — interval {Interval}ms", _options.MemoryReadIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            var state = ReadMemoryState();
            if (_processMemory.IsAttached)
            {
                var mapName = _mapNameReader.ReadCurrentMap(_processMemory, _offsetDownloader.Offsets!);
                _mapDataService.VisibilityChecker.SetActiveMap(mapName);
            }

            _triggerbot.TryTrigger(
                _processMemory,
                _processMemory.ClientBase,
                state,
                _tbState.IsEnabled,
                _tbState.PreFireFovDegrees,
                _tbState.MinReactionDelayMs,
                _tbState.MaxReactionDelayMs,
                _tbState.IsAutoStopEnabled);
            _recoilCompensator.TryCompensate(_processMemory, _processMemory.ClientBase, _rcsState.IsEnabled);
            _enemySoundTracker.Poll(state);
            _enemyLastSeenTracker.Poll(state);
            _grenadeTrajectoryTracker.Poll(_processMemory, state);
            _eventBus.PublishMemoryRead(state);
            await Task.Delay(_options.MemoryReadIntervalMs, stoppingToken);
        }
    }

    private MemoryState ReadMemoryState()
    {
        if (_entityResolver is null || !_processMemory.IsAttached)
            return MemoryState.Detached;

        try
        {
            return _entityResolver.ReadState();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Memory read failed");
            return MemoryState.Detached;
        }
    }
}
