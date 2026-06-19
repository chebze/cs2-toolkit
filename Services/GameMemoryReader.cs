using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Maps;
using Cs2Toolkit.Memory;
using Cs2Toolkit.Models;
using Cs2Toolkit.Offsets;
using Cs2Toolkit.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cs2Toolkit.Services;

public sealed class GameMemoryReader : BackgroundService
{
    private readonly ProcessMemory _processMemory;
    private readonly OffsetDownloader _offsetDownloader;
    private readonly RuntimeGate _runtimeGate;
    private readonly ToolkitEventBus _eventBus;
    private readonly EnemySoundTracker _enemySoundTracker;
    private readonly EnemyLastSeenTracker _enemyLastSeenTracker;
    private readonly ViewMatrixHolder _viewMatrixHolder;
    private readonly RecoilCompensator _recoilCompensator;
    private readonly Triggerbot _triggerbot;
    private readonly RcsState _rcsState;
    private readonly TbState _tbState;
    private readonly MapDataService _mapDataService;
    private readonly MapNameReader _mapNameReader;
    private readonly GrenadeTrajectoryTracker _grenadeTrajectoryTracker;
    private readonly EnemyEspState _enemyEspState;
    private readonly AimHelper _aimHelper;
    private readonly AimHelperState _aimHelperState;
    private readonly RuntimeConfigProvider _runtimeConfig;
    private readonly ActiveWeaponTracker _activeWeaponTracker;
    private readonly WeaponConfigState _weaponConfigState;
    private readonly RadarTracker _radarTracker;
    private readonly RadarState _radarState;
    private readonly ILogger<GameMemoryReader> _logger;

    private EntityResolver? _entityResolver;
    private ushort _lastWeaponId;
    private string? _currentMapName;

    public GameMemoryReader(
        ProcessMemory processMemory,
        OffsetDownloader offsetDownloader,
        RuntimeGate runtimeGate,
        ToolkitEventBus eventBus,
        EnemySoundTracker enemySoundTracker,
        EnemyLastSeenTracker enemyLastSeenTracker,
        ViewMatrixHolder viewMatrixHolder,
        RecoilCompensator recoilCompensator,
        Triggerbot triggerbot,
        RcsState rcsState,
        TbState tbState,
        MapDataService mapDataService,
        MapNameReader mapNameReader,
        GrenadeTrajectoryTracker grenadeTrajectoryTracker,
        EnemyEspState enemyEspState,
        AimHelper aimHelper,
        AimHelperState aimHelperState,
        RuntimeConfigProvider runtimeConfig,
        ActiveWeaponTracker activeWeaponTracker,
        WeaponConfigState weaponConfigState,
        RadarTracker radarTracker,
        RadarState radarState,
        ILogger<GameMemoryReader> logger)
    {
        _processMemory = processMemory;
        _offsetDownloader = offsetDownloader;
        _runtimeGate = runtimeGate;
        _eventBus = eventBus;
        _enemySoundTracker = enemySoundTracker;
        _enemyLastSeenTracker = enemyLastSeenTracker;
        _viewMatrixHolder = viewMatrixHolder;
        _recoilCompensator = recoilCompensator;
        _triggerbot = triggerbot;
        _rcsState = rcsState;
        _tbState = tbState;
        _mapDataService = mapDataService;
        _mapNameReader = mapNameReader;
        _grenadeTrajectoryTracker = grenadeTrajectoryTracker;
        _enemyEspState = enemyEspState;
        _aimHelper = aimHelper;
        _aimHelperState = aimHelperState;
        _runtimeConfig = runtimeConfig;
        _activeWeaponTracker = activeWeaponTracker;
        _weaponConfigState = weaponConfigState;
        _radarTracker = radarTracker;
        _radarState = radarState;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GameMemoryReader waiting for overlay and injection");

        await _runtimeGate.MemoryReaderStartTask.WaitAsync(stoppingToken);

        if (_offsetDownloader.Offsets is null)
            throw new InvalidOperationException("Offsets were not downloaded before memory reading started.");

        var options = _runtimeConfig.Current;
        _entityResolver = new EntityResolver(_processMemory, _offsetDownloader.Offsets, options.Clairvoyance);
        _enemySoundTracker.Initialize(_offsetDownloader.Offsets);
        _enemyLastSeenTracker.Initialize(_offsetDownloader.Offsets);
        _viewMatrixHolder.Initialize(_offsetDownloader.Offsets);
        _recoilCompensator.Initialize(_offsetDownloader.Offsets, options.Rcs);
        _triggerbot.Initialize(_offsetDownloader.Offsets, options.Tb, _mapDataService.VisibilityChecker);
        _aimHelper.Initialize(
            _offsetDownloader.Offsets,
            options.AimHelper,
            _mapDataService.VisibilityChecker,
            _viewMatrixHolder);
        _grenadeTrajectoryTracker.Initialize(_offsetDownloader.Offsets, _mapDataService.VisibilityChecker);
        _radarTracker.Initialize(_offsetDownloader.Offsets);
        _logger.LogInformation("GameMemoryReader started — interval {Interval}ms", options.MemoryReadIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            options = _runtimeConfig.Current;
            var state = ReadMemoryState();
            if (_processMemory.IsAttached)
            {
                _viewMatrixHolder.Update(_processMemory);
                _currentMapName = _mapNameReader.ReadCurrentMap(_processMemory, _offsetDownloader.Offsets!);
                _mapDataService.VisibilityChecker.SetActiveMap(_currentMapName);
                _activeWeaponTracker.Update(_processMemory, _offsetDownloader.Offsets!, _processMemory.ClientBase);

                if (_activeWeaponTracker.WeaponId != _lastWeaponId)
                {
                    _lastWeaponId = _activeWeaponTracker.WeaponId;
                    _weaponConfigState.Update(_lastWeaponId, _runtimeConfig);
                }
            }

            var weapon = _weaponConfigState;
            var tb = weapon.Triggerbot;
            var rcs = weapon.Rcs;
            var aim = weapon.AimHelper;

            _triggerbot.TryTrigger(
                _processMemory,
                _processMemory.ClientBase,
                state,
                _tbState.IsEnabled,
                tb.PreFireFovDegrees ?? _tbState.PreFireFovDegrees,
                tb.MinReactionDelayMs ?? _tbState.MinReactionDelayMs,
                tb.MaxReactionDelayMs ?? _tbState.MaxReactionDelayMs,
                tb.AutoStopEnabled ?? _tbState.IsAutoStopEnabled);

            _recoilCompensator.TryCompensateWithOptions(
                _processMemory,
                _processMemory.ClientBase,
                _rcsState.IsEnabled,
                rcs);

            _aimHelper.TryAim(
                _processMemory,
                _processMemory.ClientBase,
                state,
                _aimHelperState.IsEnabled,
                aim.FovDegrees ?? _aimHelperState.FovDegrees,
                AimHelperBoneParser.Parse(aim.PreferredBone ?? _aimHelperState.PreferredBone.ToString()));

            _enemySoundTracker.Poll(state);
            _enemyLastSeenTracker.Poll(state, _enemyEspState.Mode);
            _grenadeTrajectoryTracker.Poll(_processMemory, state);
            _radarState.Update(_radarTracker.BuildSnapshot(_processMemory, state, _currentMapName));
            _eventBus.PublishMemoryRead(state);
            await Task.Delay(options.MemoryReadIntervalMs, stoppingToken);
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
